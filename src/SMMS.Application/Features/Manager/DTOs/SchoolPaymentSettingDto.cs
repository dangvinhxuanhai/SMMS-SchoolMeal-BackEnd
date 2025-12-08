using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMMS.Application.Features.Manager.DTOs;
public class SchoolPaymentSettingDto
{
    public int SettingId { get; set; }
    public Guid SchoolId { get; set; }
    public byte FromMonth { get; set; }
    public byte ToMonth { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal MealPricePerDay { get; set; }
    public string? Note { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateSchoolPaymentSettingRequest
{
    public Guid SchoolId { get; set; }
    public byte FromMonth { get; set; }
    public byte ToMonth { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal MealPricePerDay { get; set; }
    public string? Note { get; set; }
}

public class UpdateSchoolPaymentSettingRequest
{
    public byte FromMonth { get; set; }
    public byte ToMonth { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal MealPricePerDay { get; set; }
    public string? Note { get; set; }
    public bool IsActive { get; set; }
}
