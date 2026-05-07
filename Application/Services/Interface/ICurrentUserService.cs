namespace Application.Services.Interface
{
    public interface ICurrentUserService
    {
        int UserId { get; }
        bool IsAdmin { get; }
    }
}
