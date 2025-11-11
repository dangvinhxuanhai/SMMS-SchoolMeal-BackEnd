using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMMS.Application.Features.Manager.Commands;
public class GenerateFinanceReportCommand
{
    public Guid SchoolId { get; set; }
    public string ReportType { get; set; } = "monthly";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
