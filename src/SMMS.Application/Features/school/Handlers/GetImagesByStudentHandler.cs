using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SMMS.Application.Features.school.DTOs;
using SMMS.Application.Features.school.Interfaces;
using SMMS.Application.Features.school.Queries;

namespace SMMS.Application.Features.school.Handlers
{
    public class GetImagesByStudentHandler
     : IRequestHandler<GetImagesByStudentQuery, List<StudentImageDto>>
    {
        private readonly IStudentImageRepository _repo;

        public GetImagesByStudentHandler(IStudentImageRepository repo)
        {
            _repo = repo;
        }

        public async Task<List<StudentImageDto>> Handle(
            GetImagesByStudentQuery request,
            CancellationToken cancellationToken)
        {
            var images = await _repo.GetImagesByStudentIdAsync(request.StudentId);

            return images.Select(i => new StudentImageDto
            {
                ImageId = i.ImageId,
                ImageUrl = i.ImageUrl,        // URL Cloudinary
                Caption = i.Caption,
                TakenAt = i.TakenAt,
                CreatedAt = i.CreatedAt
            }).ToList();
        }
    }

}
