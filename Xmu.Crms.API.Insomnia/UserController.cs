using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xmu.Crms.Shared.Exceptions;
using Xmu.Crms.Shared.Models;
using Xmu.Crms.Shared.Service;
using static Xmu.Crms.Insomnia.Utils;

namespace Xmu.Crms.Insomnia
{
    [Route("")]
    [Produces("application/json")]
    public class UserController : Controller
    {
        private readonly JwtHeader _header;
        private readonly ILoginService _loginService;
        private readonly ISchoolService _schoolService;
        private readonly IUserService _userService;

        public UserController(JwtHeader header, ILoginService loginService, IUserService userService,
            ISchoolService schoolService)
        {
            _header = header;
            _loginService = loginService;
            _userService = userService;
            _schoolService = schoolService;
        }

        [HttpGet("/me")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> GetCurrentUser()
        {
            try
            {
                var user = await _userService.GetUserByUserIdAsync(User.Id());
                user.School = await _schoolService.GetSchoolBySchoolIdAsync(user.SchoolId ?? -1);
                return Json(user, Ignoring("City", "Province", "Password"));
            }
            catch (UserNotFoundException)
            {
                return StatusCode(404, new {msg = "用户不存在"});
            }
        }

        [HttpPut("/me")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> UpdateCurrentUser([FromBody] UserInfo updated)
        {
            try
            {
                await _userService.UpdateUserByUserIdAsync(User.Id(), updated);
                return NoContent();
            }
            catch (UserNotFoundException)
            {
                return StatusCode(404, new {msg = "用户不存在"});
            }
        }

        [HttpGet("/signin")]
        public IActionResult SigninWechat([FromQuery] string code, [FromQuery] string state,
            [FromQuery(Name = "success_url")] string successUrl) => throw new NotSupportedException();

        [HttpPost("/signin")]
        public async Task<IActionResult> SigninPassword([FromBody] UsernameAndPassword uap)
        {
            try
            {
                var user = await _loginService.SignInPhoneAsync(new UserInfo {Phone = uap.Phone, Password = uap.Password});
                await HttpContext.SignInAsync(JwtBearerDefaults.AuthenticationScheme, new ClaimsPrincipal());
                return Json(CreateSigninResult(user));
            }
            catch (PasswordErrorException)
            {
                return StatusCode(401, new {msg = "用户名或密码错误"});
            }
            catch (UserNotFoundException)
            {
                return StatusCode(404, new {msg = "用户不存在"});
            }
        }

        [HttpPost("/register")]
        public async Task<IActionResult> RegisterPassword([FromBody] UsernameAndPassword uap)
        {
            try
            {
                var user = await _loginService.SignUpPhoneAsync(new UserInfo {Phone = uap.Phone, Password = uap.Password});
                return Json(CreateSigninResult(user));
            }
            catch (PhoneAlreadyExistsException)
            {
                return StatusCode(409, new {msg = "手机已注册"});
            }
        }

        private SigninResult CreateSigninResult(UserInfo user) => new SigninResult
        {
            Id = user.Id,
            Name = user.Name,
            Type = user.Type.ToString().ToLower(),
            Jwt = new JwtSecurityTokenHandler().WriteToken(new JwtSecurityToken(_header,
                new JwtPayload(
                    null,
                    null,
                    new[]
                    {
                        new Claim("id", user.Id.ToString()),
                        new Claim("type", user.Type.ToString().ToLower())
                    },
                    null,
                    DateTime.Now.AddDays(7)
                )))
        };

        [HttpPost("/upload/avatar")]
        public IActionResult UploadAvatar(IFormFile file) =>
            Created("/upload/avatar.png", new {url = "/upload/avatar.png"});

        [Route("/")]
        public IActionResult HomePage() => Redirect("/Login");

        public class UsernameAndPassword
        {
            public string Phone { get; set; }
            public string Password { get; set; }
        }

        public class SigninResult
        {
            public long Id { get; set; }

            public string Name { get; set; }

            public string Type { get; set; }

            public string Jwt { get; set; }
        }
    }
}