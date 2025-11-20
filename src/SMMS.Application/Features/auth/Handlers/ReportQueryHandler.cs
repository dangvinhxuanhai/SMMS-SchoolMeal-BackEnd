using MediatR;
using SMMS.Application.Features.auth.DTOs;
using SMMS.Application.Features.auth.Interfaces;
using SMMS.Application.Features.auth.Queries;

public class ReportQueryHandler :
    IRequestHandler<GetUserReportQuery, List<UserReportDto>>,
    IRequestHandler<GetAllUserReportQuery, List<UserReportDto>>,
    IRequestHandler<GetFinanceReportQuery, List<FinanceReportDto>>,
    IRequestHandler<GetAllFinanceReportQuery, List<FinanceReportDto>>  
{
    private readonly IReportRepository _reportRepository;

    public ReportQueryHandler(IReportRepository reportRepository)
    {
        _reportRepository = reportRepository;
    }

    public async Task<List<UserReportDto>> Handle(GetUserReportQuery request, CancellationToken cancellationToken)
    {
        return await _reportRepository.GetUserReportAsync(request.Filter);
    }

    public async Task<List<UserReportDto>> Handle(GetAllUserReportQuery request, CancellationToken cancellationToken)
    {
        return await _reportRepository.GetAllUserReportAsync();
    }

    public async Task<List<FinanceReportDto>> Handle(GetFinanceReportQuery request, CancellationToken cancellationToken)
    {
        return await _reportRepository.GetFinanceReportAsync(request.Filter);
    }

    public async Task<List<FinanceReportDto>> Handle(GetAllFinanceReportQuery request, CancellationToken cancellationToken)
    {
        return await _reportRepository.GetAllFinanceReportAsync();
    }
}
