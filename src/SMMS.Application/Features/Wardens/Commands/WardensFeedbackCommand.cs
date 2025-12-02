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
public record UpdateWardenFeedbackCommand(
       int FeedbackId,
       CreateFeedbackRequest Request
   ) : IRequest<FeedbackDto>;
// ❌ Xoá feedback (giám thị chỉ được xoá feedback của chính mình)
public record DeleteWardenFeedbackCommand(
    int FeedbackId,
    Guid WardenId
) : IRequest<bool>;
