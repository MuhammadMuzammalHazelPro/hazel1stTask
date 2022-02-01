using System.ComponentModel.DataAnnotations;

namespace WebApi.Models.Users
{
    public class AuthenticateRequest
    {
        public string Email { get; set; }
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }
    }
}