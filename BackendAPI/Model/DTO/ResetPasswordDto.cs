namespace BackendAPI.Model.DTO
{
    public class ResetPasswordDto
    {
        public string Password { get; set; }
        public string Email { get; set; }
        public string RecoveryToken { get; set; }
    }
}
