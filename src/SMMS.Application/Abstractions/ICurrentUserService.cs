namespace SMMS.Application.Abstractions
{
    public interface ICurrentUserService
    {
        Guid? UserId { get; }
        string? Email { get; }
    }
}

