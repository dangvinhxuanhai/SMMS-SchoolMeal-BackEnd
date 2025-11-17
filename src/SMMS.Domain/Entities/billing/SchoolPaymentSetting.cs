using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SMMS.Domain.Entities.school;
namespace SMMS.Domain.Entities.billing;
public partial class SchoolPaymentSetting
{
    public int SettingId { get; set; }

    public Guid SchoolId { get; set; }

    public byte FromMonth { get; set; }

    public byte ToMonth { get; set; }

    public decimal TotalAmount { get; set; }

    public string? Note { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual School School { get; set; } = null!;
}
