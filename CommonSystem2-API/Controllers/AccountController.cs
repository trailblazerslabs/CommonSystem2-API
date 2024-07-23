using CommonSystem2_API.DataModel;
using CommonSystem2_API.Middleware;
using CommonSystem2_API.Models;
using CommonSystem2_API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CommonSystem2_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [BasicAuthentication]
    public class AccountController : ControllerBase
    {
        private readonly ILogger<AccountController> _logger;
        private readonly IUserService _userService;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;

        public AccountController(IUserService userService,
            ILogger<AccountController> logger,
            IConfiguration configuration,
            IEmailService emailService)
        {
            _logger = logger;
            _configuration = configuration;
            _userService = userService;
            _emailService = emailService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserModel user)
        {
            var userData = await _userService.GetUser(user.Username);
            if (userData == null)
            {
                return Ok(new { result = false, message = "Email ID is not exists in the system" });
            }
            bool isValidPassword = CommonHelper.VerifyHmacSignature(user.Password,userData.Password,_configuration);

            if (!isValidPassword)
            {
                return Ok(new { result = false, message = "Invalid password" });
            }

            var secret = _configuration["Jwt:Secret"];
            if (secret == null || string.IsNullOrEmpty(secret))
                return Unauthorized();

            var token = GenerateJwtToken(userData, secret);

            return Ok(new { accessToken = token });
        }

        [HttpPost("signUp")]
        public async Task<IActionResult> SignUp([FromBody] UserModel user)
        {
            var userData = await _userService.GetUser(user.Username);
            if (userData != null)
            {
                return Ok(new { result = false, message = "Email ID is already exists in the system. Please try again with new email Id." });
            }
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


        [HttpPost("activateLink")]
        public async Task<IActionResult> ActivateLink([FromQuery] string guid, [FromQuery] DateTime expiryTime, [FromQuery] string signature)
        {
            var dataToSign = $"{guid}|{expiryTime:o}";
            bool isValidLink = CommonHelper.VerifyHmacSignature(dataToSign, signature, _configuration);
            if (!isValidLink || expiryTime < DateTime.UtcNow)
                return Ok(new { result = false, message = "The link is expired or invalid" });
            Guid id = Guid.Parse(guid);
            var user = await _userService.ActivateUser(id);

            var secret = _configuration["Jwt:Secret"];
            if (secret == null || string.IsNullOrEmpty(secret))
                return Unauthorized();

            if (user == null)
                return Ok(new { result = false, message = "The link is expired or invalid" });
            var token = GenerateJwtToken(user, secret);
            return Ok(new { result = (user != null), message = (user != null ? "The link is activated" : "The link is not activated"), accessToken = (user != null ? token : string.Empty) });
        }



        private string GenerateJwtToken(User user, string secret)
        {

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(secret);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(JwtRegisteredClaimNames.Jti, user.Id.ToString()),
                    new Claim(ClaimTypes.Email, user.Username),
                    new Claim(ClaimTypes.GivenName, user.Name),
                    new Claim(ClaimTypes.Role, user.Role),
                    new Claim(ClaimTypes.Upn, user.Username),
                    new Claim("organization", user.Organization)
                }),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
