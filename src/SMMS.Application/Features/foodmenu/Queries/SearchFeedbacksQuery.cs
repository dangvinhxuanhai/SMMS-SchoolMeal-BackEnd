using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using MediatR;
using SMMS.Application.Features.foodmenu.DTOs;

namespace SMMS.Application.Features.foodmenu.Queries;
public sealed record SearchFeedbacksQuery : IRequest<IReadOnlyList<FeedbackKsDto>>
{
    // Filter
    [JsonIgnore]
    public Guid? SchoolId { get; set; }
    [JsonIgnore]
    public Guid? SenderId { get; set; }
    public int? DailyMealId { get; set; }
    public string? TargetType { get; set; }
    public string? Keyword { get; set; }

    public DateTime? FromCreatedAt { get; set; }
    public DateTime? ToCreatedAt { get; set; }

    // ✨ NEW: filter rating (1–5). Null = không lọc
    public byte? Rating { get; set; }

    // Sort
    public string SortBy { get; set; } = "CreatedAt";   // CreatedAt / Sender / TargetType / Rating
    public bool SortDesc { get; set; } = true;
}
