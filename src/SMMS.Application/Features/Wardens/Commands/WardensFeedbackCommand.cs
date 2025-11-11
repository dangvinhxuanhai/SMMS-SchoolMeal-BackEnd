using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SMMS.Application.Features.Wardens.DTOs;

namespace SMMS.Application.Features.Wardens.Commands;
// 🟡 Tạo feedback mới
public record CreateWardenFeedbackCommand(CreateFeedbackRequest Request)
    : IRequest<FeedbackDto>;
