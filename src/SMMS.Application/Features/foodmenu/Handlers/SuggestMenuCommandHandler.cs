using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Azure;
using MediatR;
using SMMS.Application.Features.foodmenu.Commands;
using SMMS.Application.Features.foodmenu.DTOs;
using SMMS.Application.Features.foodmenu.Interfaces;
using SMMS.Application.Features.school.Interfaces;
using SMMS.Domain.Entities.rag;

namespace SMMS.Application.Features.foodmenu.Handlers;
public class SuggestMenuCommandHandler
        : IRequestHandler<SuggestMenuCommand, AiMenuRecommendResponse>
{
    private readonly IAiMenuClient _aiMenuClient;
    private readonly IClassStudentRepository _classStudentRepo;
    private readonly IMenuRecommendSessionRepository _sessionRepo;
    private readonly IMenuRecommendResultRepository _resultRepo;
    private readonly IStudentHealthRepository _studentHealthRepo;

    private const double DefaultMainKcal = 650;
    private const double DefaultSideKcal = 350;

    public SuggestMenuCommandHandler(
        IAiMenuClient aiMenuClient,
        IClassStudentRepository classStudentRepo,
        IMenuRecommendSessionRepository sessionRepo,
        IMenuRecommendResultRepository resultRepo,
        IStudentHealthRepository studentHealthRepo)
    {
        _aiMenuClient = aiMenuClient;
        _classStudentRepo = classStudentRepo;
        _sessionRepo = sessionRepo;
        _resultRepo = resultRepo;
        _studentHealthRepo = studentHealthRepo;
    }

    public async Task<AiMenuRecommendResponse> Handle(
        SuggestMenuCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Tính MaxKcal (dựa trên request hoặc BMI)
        var (maxMainKcal, maxSideKcal) =
            await ResolveMaxKcalAsync(
                request.SchoolId,
                request.MaxMainKcal,
                request.MaxSideKcal,
                cancellationToken);

        var aiRequest = new AiMenuRecommendRequest
        {
            UserId = request.UserId,
            SchoolId = request.SchoolId,
            MainIngredientIds = request.MainIngredientIds,
            SideIngredientIds = request.SideIngredientIds,
            AvoidAllergenIds = request.AvoidAllergenIds,
            MaxMainKcal = request.MaxMainKcal,
            MaxSideKcal = request.MaxSideKcal,
            TopKMain = request.TopKMain,
            TopKSide = request.TopKSide
        };

        var aiResponse = await _aiMenuClient.RecommendAsync(aiRequest, cancellationToken);

        var allDishes = (aiResponse.RecommendedMain?.Count ?? 0)
                          + (aiResponse.RecommendedSide?.Count ?? 0);

        // 3) LƯU MENU_RECOMMEND_SESSION
        var requestJson = JsonSerializer.Serialize(aiRequest);

        var session = new MenuRecommendSession
        {
            // mapping đúng với entity của anh
            UserId = request.UserId,
            CreatedAt = DateTime.UtcNow,
            RequestJson = requestJson,
            CandidateCount = allDishes,
            ModelVersion = "v1.0"
        };

        var sessionId = await _sessionRepo.AddAsync(session, cancellationToken); // sau lệnh này session.SessionId có giá trị

        // 4) LƯU MENU_RECOMMEND_RESULTS
        var results = new List<MenuRecommendResult>();

        int rank = 1;
        foreach (var dish in aiResponse.RecommendedMain ?? Enumerable.Empty<AiDishDto>())
        {
            results.Add(new MenuRecommendResult
            {
                SessionId = session.SessionId,
                FoodId = dish.FoodId,
                IsMain = true,
                RankShown = rank++,
                Score = dish.Score,
                IsChosen = false
            });
        }

        rank = 1;
        foreach (var dish in aiResponse.RecommendedSide ?? Enumerable.Empty<AiDishDto>())
        {
            results.Add(new MenuRecommendResult
            {
                SessionId = session.SessionId,
                FoodId = dish.FoodId,
                IsMain = false,
                RankShown = rank++,
                Score = dish.Score,
                IsChosen = false
            });
        }

        if (results.Count > 0)
            await _resultRepo.AddRangeAsync(results, cancellationToken);


        return new AiMenuRecommendResponse
        {
            SessionId = sessionId,
            RecommendedMain = aiResponse.RecommendedMain,
            RecommendedSide = aiResponse.RecommendedSide
        };
    }

    private async Task<(double Main, double Side)> ResolveMaxKcalAsync(
    Guid schoolId,
    double? maxMainFromRequest,
    double? maxSideFromRequest,
    CancellationToken ct)
    {
        // FE truyền đủ → ưu tiên
        if (maxMainFromRequest is > 0 && maxSideFromRequest is > 0)
            return (maxMainFromRequest.Value, maxSideFromRequest.Value);

        // Tính theo BMI trung bình lớp đầu tiên
        var (bmiMain, bmiSide) = await EstimateKcalFromSchoolAsync(schoolId, ct);

        var main = maxMainFromRequest is > 0 ? maxMainFromRequest.Value : bmiMain;
        var side = maxSideFromRequest is > 0 ? maxSideFromRequest.Value : bmiSide;

        return (main, side);
    }

    private async Task<(double Main, double Side)> EstimateKcalFromSchoolAsync(
        Guid schoolId,
        CancellationToken ct)
    {
        var avgBmi = await _studentHealthRepo.GetAverageBmiForFirstClassAsync(schoolId, ct);

        if (avgBmi is null)
            return (DefaultMainKcal, DefaultSideKcal);

        double main = DefaultMainKcal;
        double side = DefaultSideKcal;

        if (avgBmi < 14)            // gầy → tăng khẩu phần
        {
            main += 100;
            side += 50;
        }
        else if (avgBmi > 18)       // dư cân → giảm nhẹ
        {
            main -= 100;
            side -= 50;
        }

        return (main, side);
    }
}
