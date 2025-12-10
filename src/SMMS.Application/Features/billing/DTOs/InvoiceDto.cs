using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace SMMS.Application.Features.billing.DTOs
{
    public class InvoiceDto
    {
        public long InvoiceId { get; set; }
        public Guid InvoiceCode { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public short MonthNo { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
        public int AbsentDay { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal AmountToPay { get; set; }
    }
    public class InvoiceDetailDto
    {
        public long InvoiceId { get; set; }
        public Guid InvoiceCode { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string ClassName { get; set; } = string.Empty;
        public string SchoolName { get; set; } = string.Empty;
        public short MonthNo { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
        public int AbsentDay { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal AmountToPay { get; set; }
        public string SettlementBankCode { get; set; } = string.Empty;     // Mã ngân hàng
        public string SettlementAccountNo { get; set; } = string.Empty;     // Số tài khoản
        public string SettlementAccountName { get; set; } = string.Empty;  // Chủ tài khoản
    }

}
