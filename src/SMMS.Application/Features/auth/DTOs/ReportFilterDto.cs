using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMMS.Application.Features.auth.DTOs
{
    public class ReportFilterDto
    {
        public string ReportType { get; set; } = "NguoiDung";
        public string Scope { get; set; } = "ToanHeThong";
        public Guid? SchoolId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
    public class UserReportDto
    {
        public string RoleName { get; set; } = null!;
        public string? SchoolName { get; set; }
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int InactiveUsers { get; set; }
    }
    public class FinanceReportDto
    {
        public string SchoolName { get; set; }
        public decimal TotalRevenue { get; set; }
        public int RevenueCount { get; set; }
    }
    public class FinanceReportFilterDto
    {
        public Guid? SchoolId { get; set; }
        public string? Scope { get; set; } = "TatCa"; // TatCa | TheoTruong
        public DateOnly? FromDate { get; set; }
        public DateOnly? ToDate { get; set; }
    }

}
