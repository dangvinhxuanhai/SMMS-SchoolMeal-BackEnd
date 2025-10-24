using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMMS.Application.Features.Manager.DTOs;
public class ManagerOverviewDto
{
    public int TeacherCount { get; set; }
    public int StudentCount { get; set; }
    public int ClassCount { get; set; }
    public decimal FinanceThisMonth { get; set; }
    public decimal FinanceLastMonth { get; set; }
    public double FinanceChangePercent { get; set; }
}
