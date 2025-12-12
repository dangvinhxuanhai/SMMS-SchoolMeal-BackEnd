using MediatR;
using SMMS.Application.Features.billing.DTOs;
using SMMS.Application.Features.Manager.Commands;
using SMMS.Application.Features.Manager.DTOs;
using SMMS.Application.Features.Manager.Interfaces;
using SMMS.Application.Features.Manager.Queries;
using SMMS.Domain.Entities.billing; // Notification, NotificationRecipient

namespace SMMS.Application.Features.Manager.Handlers;

public class ManagerNotificationHandler :
    IRequestHandler<CreateManagerNotificationCommand, ManagerNotificationDto>,
    IRequestHandler<UpdateManagerNotificationCommand, ManagerNotificationDto>,
    IRequestHandler<DeleteManagerNotificationCommand, bool>,
    IRequestHandler<GetManagerNotificationByIdQuery, ManagerNotificationDto?>,
    IRequestHandler<GetManagerNotificationsBySenderQuery, PagedResult<ManagerNotificationDto>>
{
    private readonly IManagerNotificationRepository _repo;
    private readonly INotificationRealtimeService _realtime;

    public ManagerNotificationHandler(
        IManagerNotificationRepository repo,
        INotificationRealtimeService realtime)
    {
        _repo = repo;
        _realtime = realtime;
    }

    // 1️⃣ CREATE
    public async Task<ManagerNotificationDto> Handle(
        CreateManagerNotificationCommand command,
        CancellationToken cancellationToken)
    {
        var req = command.Request;
        var schoolId = command.SchoolId;

        if (!req.SendToParents && !req.SendToKitchenStaff && !req.SendToTeachers)
            throw new InvalidOperationException("Phải chọn ít nhất một nhóm người nhận.");

        // 1. Xác định role cần gửi
        var roleNames = new List<string>();
        if (req.SendToParents) roleNames.Add("Parent");
        if (req.SendToKitchenStaff) roleNames.Add("KitchenStaff");
        if (req.SendToTeachers) roleNames.Add("Teacher");

        // 2. Lấy danh sách user nhận trong trường
        var userIds = await _repo.GetRecipientUserIdsAsync(schoolId, roleNames);
        if (!userIds.Any())
            throw new InvalidOperationException("Không tìm thấy người nhận trong trường.");

        // 3. Tạo entity Notification
        var notif = new Notification
        {
            Title = req.Title,
            Content = req.Content,
            AttachmentUrl = req.AttachmentUrl,
            SenderId = command.SenderId,
            SendType = req.SendType, // "Immediate" | "Scheduled" | "Recurring"
            ScheduleCron = req.ScheduleCron,
            CreatedAt = DateTime.UtcNow,
        };

        await _repo.AddNotificationAsync(notif);
        await _repo.SaveChangesAsync();

        // 4. Tạo NotificationRecipients
        var recEntities = userIds.Select(uid => new NotificationRecipient
        {
            NotificationId = notif.NotificationId, UserId = uid, IsRead = false
        }).ToList();

        await _repo.AddRecipientsAsync(recEntities);
        await _repo.SaveChangesAsync();

        // 5. Map DTO trả về
        var dto = new ManagerNotificationDto
        {
            NotificationId = notif.NotificationId,
            SenderId = notif.SenderId,
            Title = notif.Title,
            Content = notif.Content,
            AttachmentUrl = notif.AttachmentUrl,
            SendType = notif.SendType,
            ScheduleCron = notif.ScheduleCron,
            CreatedAt = notif.CreatedAt,
            TotalRecipients = userIds.Count,
        };

        // 6. 🔔 realtime: gửi tới Parent / KitchenStaff / Teacher
        await _realtime.SendToUsersAsync(userIds, dto);

        return dto;
    }

    // 2️⃣ UPDATE
    public async Task<ManagerNotificationDto> Handle(
        UpdateManagerNotificationCommand command,
        CancellationToken cancellationToken)
    {
        var notif = await _repo.GetByIdAsync(command.NotificationId);
        if (notif == null)
            throw new InvalidOperationException("Không tìm thấy thông báo.");

        if (notif.SenderId != command.SenderId)
            throw new InvalidOperationException("Bạn không được phép sửa thông báo này.");

        // cập nhật nội dung
        notif.Title = command.Request.Title;
        notif.Content = command.Request.Content;
        notif.AttachmentUrl = command.Request.AttachmentUrl;
        notif.SendType = command.Request.SendType;
        notif.ScheduleCron = command.Request.ScheduleCron;

        await _repo.UpdateAsync(notif);
        await _repo.SaveChangesAsync();

        var totalRecipients = await _repo.CountRecipientsAsync(notif.NotificationId);

        return new ManagerNotificationDto
        {
            NotificationId = notif.NotificationId,
            SenderId = notif.SenderId,
            Title = notif.Title,
            Content = notif.Content,
            AttachmentUrl = notif.AttachmentUrl,
            SendType = notif.SendType,
            ScheduleCron = notif.ScheduleCron,
            CreatedAt = notif.CreatedAt,
            TotalRecipients = totalRecipients,
        };
    }

    // 3️⃣ DELETE
    public async Task<bool> Handle(
        DeleteManagerNotificationCommand command,
        CancellationToken cancellationToken)
    {
        var notif = await _repo.GetByIdAsync(command.NotificationId);
        if (notif == null)
            return false;

        if (notif.SenderId != command.SenderId)
            throw new InvalidOperationException("Bạn không được phép xoá thông báo này.");

        // 🔹 1. Lấy tất cả recipients của notification này
        var recipients = await _repo.GetRecipientsAsync(notif.NotificationId);

        // 🔹 2. Xoá hết recipients trước
        if (recipients.Any())
        {
            await _repo.DeleteRecipientsAsync(recipients);
        }

        // 🔹 3. Xoá notification
        await _repo.DeleteAsync(notif);

        // 🔹 4. SaveChanges một lần
        await _repo.SaveChangesAsync();

        // 🔔 5. Realtime thông báo client (nếu cần)
        await _realtime.BroadcastDeletedAsync(notif.NotificationId);

        return true;
    }

    // 4️⃣ GET BY ID
    public async Task<ManagerNotificationDto?> Handle(
        GetManagerNotificationByIdQuery query,
        CancellationToken cancellationToken)
    {
        var notif = await _repo.GetByIdAsync(query.NotificationId);
        if (notif == null)
            return null;

        var totalRecipients = await _repo.CountRecipientsAsync(notif.NotificationId);

        return new ManagerNotificationDto
        {
            NotificationId = notif.NotificationId,
            SenderId = notif.SenderId,
            Title = notif.Title,
            Content = notif.Content,
            AttachmentUrl = notif.AttachmentUrl,
            SendType = notif.SendType,
            ScheduleCron = notif.ScheduleCron,
            CreatedAt = notif.CreatedAt,
            TotalRecipients = totalRecipients,
        };
    }

    // 5️⃣ GET LIST BY SENDER
    public async Task<PagedResult<ManagerNotificationDto>> Handle(
        GetManagerNotificationsBySenderQuery query,
        CancellationToken cancellationToken)
    {
        var totalCount = await _repo.CountBySenderAsync(query.SenderId);

        var notifs = await _repo.GetBySenderAsync(
            query.SenderId, query.Page, query.PageSize);

        var resultList = new List<ManagerNotificationDto>();

        if (notifs.Any())
        {
            foreach (var n in notifs)
            {
                var totalRecipients = await _repo.CountRecipientsAsync(n.NotificationId);

                resultList.Add(new ManagerNotificationDto
                {
                    NotificationId = n.NotificationId,
                    SenderId = n.SenderId,
                    Title = n.Title,
                    Content = n.Content,
                    AttachmentUrl = n.AttachmentUrl,
                    SendType = n.SendType,
                    ScheduleCron = n.ScheduleCron,
                    CreatedAt = n.CreatedAt,
                    TotalRecipients = totalRecipients,
                });
            }
        }

        return new PagedResult<ManagerNotificationDto>(resultList, totalCount);
    }
}
