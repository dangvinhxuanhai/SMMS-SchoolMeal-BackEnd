using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMMS.Application.Features.foodmenu.DTOs
{
    public record CreateFeedbackDto(
     Guid SenderId,
     byte? Rating,
     string TargetType,
     string TargetRef,
     string Content,
     int? DailyMealId
 );
    public record FeedbackDto(
    int FeedbackId,
    Guid SenderId,
     byte? Rating,
    string TargetType,
    string TargetRef,
    string Content,
    DateTime CreatedAt,
    int? DailyMealId
);
    public record CreateFeedbackRequestDto(
       string Content,
       byte? Rating,
       int? DailyMealId
   );
}
