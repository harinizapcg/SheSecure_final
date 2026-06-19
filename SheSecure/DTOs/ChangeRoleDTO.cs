namespace SheSecure.AuthService.DTOs
{
    public class ChangeRoleDTO
    {
        public string UserId { get; set; } = string.Empty;
        public string NewRole { get; set; } = string.Empty;
    }
}
