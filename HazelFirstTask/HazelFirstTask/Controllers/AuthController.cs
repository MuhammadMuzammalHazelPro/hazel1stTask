using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApi.Models.Users;
using WebApi.Repository;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private IUserRepository _userRepository;
        public AuthController(IUserRepository userRepository)
        {
            _userRepository = userRepository;

        }
        [HttpPost("Login")]
        public IActionResult Login(AuthenticateRequest model)
        {
            var response = _userRepository.Authenticate(model);
            return Ok(response);
        }

        [HttpPost("register")]
        public IActionResult Register(RegisterRequest model)
        {
            _userRepository.Register(model);
            return Ok(new { message = "Registration successful" });
        }
    }
}
