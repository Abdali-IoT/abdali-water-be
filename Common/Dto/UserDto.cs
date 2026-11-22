namespace Common.Dto;

public record UserDto(
    Guid Id, 
    string Name, 
    string Email, 
    DateTime CreatedAt, 
    string GoogleSubject, 
    bool GoogleSignInEnabled,
    IReadOnlyList<DeviceDto> Devices);