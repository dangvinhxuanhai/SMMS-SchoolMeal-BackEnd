using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMMS.Application.Features.billing.DTOs
{
    public class InvoiceDto
    {
        public long InvoiceId { get; set; }
        public Guid StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public short MonthNo { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
        public int AbsentDay { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
