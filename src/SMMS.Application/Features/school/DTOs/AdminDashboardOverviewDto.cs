using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMMS.Application.Features.school.DTOs
{
        public class DashboardOverviewDto
        {
            public int TotalSchools { get; set; }
            public int TotalStudents { get; set; }
            public decimal CurrentMonthRevenue { get; set; }

            public decimal SchoolGrowth { get; set; }
            public decimal StudentGrowth { get; set; }
            public decimal RevenueGrowth { get; set; }
            public decimal PreviousMonthRevenue { get; set; }

        public List<RecentActivityDto> RecentActivities { get; set; } = new();
        }

        public class RecentActivityDto
        {
            public string Title { get; set; } = string.Empty;
            public string Icon { get; set; } = string.Empty;
            public DateTime CreatedAt { get; set; }
        }
}
