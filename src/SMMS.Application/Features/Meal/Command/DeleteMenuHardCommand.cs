using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;

namespace SMMS.Application.Features.Meal.Command;
public sealed record DeleteMenuHardCommand(int MenuId) : IRequest;
