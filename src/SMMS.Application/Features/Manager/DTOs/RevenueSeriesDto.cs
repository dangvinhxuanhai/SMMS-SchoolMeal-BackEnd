using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMMS.Application.Features.Manager.DTOs;
public class RevenuePointDto
{
    public DateTime Date { get; set; }
    public decimal Amount { get; set; }
}

public class RevenueSeriesDto
{
    public DateTime From { get; set; }
    public DateTime To { get; set; }
    public string Granularity { get; set; } = "daily";
    public List<RevenuePointDto> Points { get; set; } = new();
    public decimal Total { get; set; }
}
