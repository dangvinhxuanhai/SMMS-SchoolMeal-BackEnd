using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SMMS.Application.Features.Wardens.DTOs;

namespace SMMS.Application.Features.Wardens.Interfaces
{
    public interface IWardensFeedbackService
    {
        Task<IEnumerable<FeedbackDto>> GetFeedbacksByWardenAsync(Guid wardenId);
        Task<FeedbackDto> CreateFeedbackAsync(CreateFeedbackRequest request);
    }
}
