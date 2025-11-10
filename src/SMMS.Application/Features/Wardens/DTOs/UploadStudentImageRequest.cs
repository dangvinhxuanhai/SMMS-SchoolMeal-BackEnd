using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace SMMS.Application.Features.Wardens.DTOs;
public class UploadStudentImageRequest
{
    [Required]
    public IFormFile File { get; set; }

    [Required]
    public Guid StudentId { get; set; }

    [Required]
    public Guid UploaderId { get; set; }

    public string? Caption { get; set; }
}
public class CloudImageDto
{
    public string Url { get; set; } = string.Empty;
    public string PublicId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class UploadImageResultDto
{
    public string Url { get; set; } = string.Empty;
    public string PublicId { get; set; } = string.Empty;
}
