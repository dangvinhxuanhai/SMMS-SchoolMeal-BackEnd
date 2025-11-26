using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SMMS.Application.Features.Wardens.DTOs;

namespace SMMS.Application.Features.Wardens.Queries;
// 🟡 1. Lấy tất cả ảnh (option filter theo folder)
public record GetAllImagesQuery(string? Folder, int MaxResults = 100)
    : IRequest<List<CloudImageDto>>;

// 🟡 2. Lấy ảnh theo lớp
public record GetImagesByClassQuery(Guid ClassId, int MaxResults = 100)
    : IRequest<List<CloudImageDto>>;
