using CommonSystem2_API.DataModel;
using CommonSystem2_API.Models;
using CommonSystem2_API.Services;
using Microsoft.AspNetCore.Mvc;

namespace CommonSystem2_API.Controllers
{
    public class UserController : BaseController
    {
        private readonly ILogger<UserController> _logger;
        private readonly IUserService _userService;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;

        public UserController(IUserService userService,
            ILogger<UserController> logger,
            IConfiguration configuration,
            IEmailService emailService)
        {
            _logger = logger;
            _configuration = configuration;
            _userService = userService;
            _emailService = emailService;
        }

        [HttpGet("resendLink")]
        public async Task<IActionResult> ResendLink([FromQuery] string username)
        {
            var user = await _userService.GetUser(username);
            if (user != null)
            {
                string subject = "Welcome";
                var body = CommonHelper.GetInviteEmailBody(user, _configuration);
                bool result = await _emailService.SendEmailAsync(user.Username, subject, body);
                return Ok(new { result = result, link = body });
            }
            else
            {
                return Ok(new { result = false });
            }
        }

        [HttpGet("getUsers")]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _userService.GetUsers();
            return Ok(new { result = (users != null && users.Count() > 0), users = users });
        }


        [HttpPost("addUser")]
        public async Task<IActionResult> AddUser([FromBody] UserModel user)
        {
            user.Password = CommonHelper.GenerateHmacSignature(user.Password, _configuration);
            var dbUser = await _userService.AddUser(user);
            if (dbUser != null)
            {
                string subject = "Welcome";
                var body = CommonHelper.GetInviteEmailBody(dbUser, _configuration);
                bool result = await _emailService.SendEmailAsync(user.Username, subject, body);
                return Ok(new { result = result, link = body });
            }
            else
            {
                return Ok(new { result = false });
            }
        }

        [HttpPost("updateUser")]
        public async Task<IActionResult> UpdateUser([FromBody] User user)
        {
            var dbUser = await _userService.UpdateUser(user);
            return Ok(new { result = (dbUser != null), data = dbUser });
        }
    }
}
