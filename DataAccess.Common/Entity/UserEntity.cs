namespace DataAccess.Common.Entity;

public class UserEntity
{
    public Guid Id { get; init; }
    public required string Name { get; init; }
    public string Email { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public string GoogleSubject { get; init; } = string.Empty;
    public bool GoogleSignInEnabled { get; init; }

    public List<DeviceEntity> Devices { get; init; } = [];
}