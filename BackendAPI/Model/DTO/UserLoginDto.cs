using System.ComponentModel.DataAnnotations;

namespace BackendAPI.Model.DTO
{
    public class UserLoginDto
    {
        public string Email { get; set; }

        public string Password { get; set; }
    }
}
