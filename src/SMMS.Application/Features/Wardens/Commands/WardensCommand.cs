using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SMMS.Application.Features.Wardens.DTOs;

namespace SMMS.Application.Features.Wardens.Commands;
public record CreateStudentBmiCommand(
       Guid StudentId,
       double HeightCm,
       double WeightKg,
       DateTime RecordDate
   ) : IRequest<StudentHealthDto>;
public record UpdateStudentBmiCommand(
       Guid RecordId,
       double HeightCm,
       double WeightKg,
       DateTime RecordDate
   ) : IRequest<StudentHealthDto?>;
// Xoá 1 record BMI
public record DeleteStudentBmiCommand(Guid RecordId)
    : IRequest<bool>;
