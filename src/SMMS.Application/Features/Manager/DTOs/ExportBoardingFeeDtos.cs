using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMMS.Application.Features.Manager.DTOs;
public class ExportStudentFeeRowDto
{
    public int No { get; set; }
    public Guid StudentId { get; set; }
    public string StudentName { get; set; } = default!;

    // Cột 1
    public decimal PrevDeduction { get; set; }   // prevPerDay * (holidayPrev + absentPrevStudent)

    // Cột 2
    public decimal SettingTotalAmount { get; set; } // SchoolPaymentSettings.TotalAmount (tháng hiện tại)

    // Cột 3
    public decimal InvoiceTotalPrice { get; set; }  // Invoice.TotalPrice (tháng hiện tại)
}


public sealed class ExportFeeBoardResult
{
    public string FileName { get; set; } = "export.xlsx";
    public byte[] Content { get; set; } = Array.Empty<byte>();
    public string ContentType { get; set; }
        = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
}
