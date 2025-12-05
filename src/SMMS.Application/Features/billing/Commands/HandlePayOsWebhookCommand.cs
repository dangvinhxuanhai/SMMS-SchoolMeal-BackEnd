using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SMMS.Application.Features.billing.DTOs;

namespace SMMS.Application.Features.billing.Commands;
public sealed record HandlePayOsWebhookCommand(
    PayOsWebhookPayload Payload
) : IRequest;
