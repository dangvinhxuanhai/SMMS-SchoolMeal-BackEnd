using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMMS.Application.Features.Manager.Queries;
public class GetAccountsQuery
{
    public string Role { get; set; } = string.Empty;
    public bool? IsActive { get; set; }
}
