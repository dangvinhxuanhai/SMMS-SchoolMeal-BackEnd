using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SMMS.Application.Features.Meal.DTOs;

namespace SMMS.Application.Features.Meal.Queries;
public class GetMenuListQuery : IRequest<List<KsMenuListItemDto>>
{
    public Guid SchoolId { get; set; }   // set từ token
    public int? YearId { get; set; }     // optional filter
    public short? WeekNo { get; set; }   // optional filter
}
