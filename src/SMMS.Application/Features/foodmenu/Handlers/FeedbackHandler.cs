using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SMMS.Application.Features.foodmenu.Commands;
using SMMS.Application.Features.foodmenu.DTOs;
using SMMS.Application.Features.foodmenu.Interfaces;
using SMMS.Application.Features.foodmenu.Queries;

namespace SMMS.Application.Features.foodmenu.Handlers
{
    public class FeedbackHandler :
      IRequestHandler<CreateFeedbackCommand, FeedbackDto>,
      IRequestHandler<GetFeedbackBySenderQuery, IReadOnlyList<FeedbackDto>>
    {
        private readonly IFeedbackRepository _repo;

        public FeedbackHandler(IFeedbackRepository repo)
        {
            _repo = repo;
        }

        public async Task<FeedbackDto> Handle(CreateFeedbackCommand request, CancellationToken ct)
        {
            return await _repo.CreateAsync(request.Dto, ct);
        }

        public async Task<IReadOnlyList<FeedbackDto>> Handle(GetFeedbackBySenderQuery request, CancellationToken ct)
        {
            return await _repo.GetBySenderAsync(request.SenderId, ct);
        }
    }
}
