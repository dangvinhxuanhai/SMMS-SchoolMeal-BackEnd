using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace SMMS.Application.Features.billing.DTOs
{
    public class CreateSchoolRevenueDto
    {
        public Guid SchoolId { get; set; }
        public DateOnly RevenueDate { get; set; }
        public decimal RevenueAmount { get; set; }
        public string? ContractCode { get; set; }
        public string? ContractNote { get; set; }

        public IFormFile? ContractFile { get; set; }  // upload file
    }
    public class UpdateSchoolRevenueDto
    {
        public DateOnly RevenueDate { get; set; }
        public decimal RevenueAmount { get; set; }
        public string? ContractCode { get; set; }
        public string? ContractNote { get; set; }

        public IFormFile? ContractFile { get; set; }  // upload file mới
    }
}
