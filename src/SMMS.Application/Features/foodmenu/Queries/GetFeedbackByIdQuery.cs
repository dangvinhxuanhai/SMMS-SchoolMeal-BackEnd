using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SMMS.Application.Features.foodmenu.DTOs;

namespace SMMS.Application.Features.foodmenu.Queries;
public sealed record GetFeedbackByIdQuery(int FeedbackId)
    : IRequest<FeedbackKsDto?>;
