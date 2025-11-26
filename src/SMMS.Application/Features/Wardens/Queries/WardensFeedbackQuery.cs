using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SMMS.Application.Features.Wardens.DTOs;

namespace SMMS.Application.Features.Wardens.Queries;

// 🟢 Lấy feedback theo giám thị
public record GetWardenFeedbacksQuery(Guid WardenId)
    : IRequest<IEnumerable<FeedbackDto>>;
