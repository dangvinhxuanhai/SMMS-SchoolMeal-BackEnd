using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SMMS.Application.Features.foodmenu.Commands;
using SMMS.Application.Features.foodmenu.Interfaces;

namespace SMMS.Application.Features.foodmenu.Handlers;
public sealed class GenerateAiFilesCommandHandler
    : IRequestHandler<GenerateAiFilesCommand>
{
    private readonly IAiMenuAdminClient _aiMenuAdminClient;

    public GenerateAiFilesCommandHandler(IAiMenuAdminClient aiMenuAdminClient)
    {
        _aiMenuAdminClient = aiMenuAdminClient;
    }

    public async Task Handle(GenerateAiFilesCommand request, CancellationToken cancellationToken)
    {
        // Gọi sang Python để build index + graph cho tất cả trường NeedRebuildAiIndex = 0
        await _aiMenuAdminClient.RebuildForPendingSchoolsAsync(cancellationToken);
    }
}
