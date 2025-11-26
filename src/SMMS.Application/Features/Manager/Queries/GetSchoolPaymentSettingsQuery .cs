using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SMMS.Application.Features.Manager.DTOs;

namespace SMMS.Application.Features.Manager.Queries;
public class GetSchoolPaymentSettingsQuery
        : IRequest<List<SchoolPaymentSettingDto>>
{
    public Guid SchoolId { get; }

    public GetSchoolPaymentSettingsQuery(Guid schoolId)
    {
        SchoolId = schoolId;
    }
}

public class GetSchoolPaymentSettingByIdQuery
    : IRequest<SchoolPaymentSettingDto?>
{
    public int SettingId { get; }

    public GetSchoolPaymentSettingByIdQuery(int settingId)
    {
        SettingId = settingId;
    }
}
