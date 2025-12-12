using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMMS.Application.Features.foodmenu.Interfaces;
public interface IAiMenuAdminClient
{
    Task RebuildForPendingSchoolsAsync(CancellationToken ct = default);
}

