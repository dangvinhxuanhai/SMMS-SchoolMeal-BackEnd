using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMMS.Application.Features.Manager.DTOs;
public class GenerateSchoolInvoicesRequest
{
    /// <summary>Ngày bắt đầu kỳ tính tiền (chỉ lấy phần Date)</summary>
    public DateTime DateFrom { get; set; }

    /// <summary>Ngày kết thúc kỳ tính tiền (chỉ lấy phần Date)</summary>
    public DateTime DateTo { get; set; }

    /// Bỏ qua học sinh đã có invoice cho kỳ này?
    public bool SkipExisting { get; set; } = true;
}

public class UpdateInvoiceRequest
{
    public short MonthNo { get; set; }
    public DateTime DateFrom { get; set; }
    public DateTime DateTo { get; set; }
    public int AbsentDay { get; set; }
    public string Status { get; set; } = "Unpaid"; // 'Unpaid','Paid','Refunded'
}
public class InvoiceDto1
{
    public long InvoiceId { get; set; }
    public Guid InvoiceCode { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public Guid StudentId { get; set; }
    public short MonthNo { get; set; }
    public DateTime DateFrom { get; set; }
    public DateTime DateTo { get; set; }
    public int AbsentDay { get; set; }
    public string Status { get; set; } = string.Empty;
}
