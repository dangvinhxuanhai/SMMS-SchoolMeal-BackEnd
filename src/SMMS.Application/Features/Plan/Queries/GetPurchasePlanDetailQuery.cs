using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SMMS.Application.Features.Plan.DTOs;

namespace SMMS.Application.Features.Plan.Queries;
public sealed record GetPurchasePlanDetailQuery(int PlanId) : IRequest<PurchasePlanDto?>;
