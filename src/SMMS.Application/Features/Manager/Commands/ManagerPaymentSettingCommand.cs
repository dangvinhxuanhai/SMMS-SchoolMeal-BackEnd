using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SMMS.Application.Features.Manager.DTOs;

namespace SMMS.Application.Features.Manager.Commands;
public class CreateSchoolPaymentSettingCommand
        : IRequest<SchoolPaymentSettingDto>
{
    public CreateSchoolPaymentSettingRequest Request { get; }

    public CreateSchoolPaymentSettingCommand(CreateSchoolPaymentSettingRequest request)
    {
        Request = request;
    }
}

public class UpdateSchoolPaymentSettingCommand
    : IRequest<SchoolPaymentSettingDto?>
{
    public int SettingId { get; }
    public UpdateSchoolPaymentSettingRequest Request { get; }

    public UpdateSchoolPaymentSettingCommand(
        int settingId,
        UpdateSchoolPaymentSettingRequest request)
    {
        SettingId = settingId;
        Request = request;
    }
}

public class DeleteSchoolPaymentSettingCommand : IRequest<bool>
{
    public int SettingId { get; }

    public DeleteSchoolPaymentSettingCommand(int settingId)
    {
        SettingId = settingId;
    }
}
