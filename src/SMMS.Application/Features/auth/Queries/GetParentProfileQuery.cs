using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SMMS.Application.Features.auth.DTOs;

namespace SMMS.Application.Features.auth.Queries
{
    public class GetParentProfileQuery : IRequest<UserProfileResponseDto>
    {
        public Guid ParentId { get; set; }

        public GetParentProfileQuery(Guid parentId)
        {
            ParentId = parentId;
        }
    }
}
