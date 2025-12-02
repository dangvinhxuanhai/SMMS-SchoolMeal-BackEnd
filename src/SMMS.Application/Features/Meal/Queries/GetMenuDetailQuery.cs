using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SMMS.Application.Features.Meal.DTOs;

namespace SMMS.Application.Features.Meal.Queries;
public class GetMenuDetailQuery : IRequest<KsMenuDetailDto>
{
    public int MenuId { get; set; }
    public Guid SchoolId { get; set; }  // từ token
}
