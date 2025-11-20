using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SMMS.Application.Features.foodmenu.DTOs;

namespace SMMS.Application.Features.foodmenu.Commands
{
    public record CreateFeedbackCommand(CreateFeedbackDto Dto) : IRequest<FeedbackDto>;
}
