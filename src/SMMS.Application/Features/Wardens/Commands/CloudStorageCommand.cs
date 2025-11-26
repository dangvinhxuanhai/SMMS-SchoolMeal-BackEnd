using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SMMS.Application.Features.Wardens.DTOs;

namespace SMMS.Application.Features.Wardens.Commands;
// 🟢 3. Upload ảnh học sinh
public record UploadStudentImageCommand(UploadStudentImageRequest Request, string? BaseFolder = "student_images")
    : IRequest<UploadImageResultDto>;

// 🧹 4. Xóa ảnh theo publicId
public record DeleteImageCommand(string PublicId)
    : IRequest<bool>;
