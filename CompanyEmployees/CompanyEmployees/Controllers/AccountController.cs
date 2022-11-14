using AutoMapper;
using CompanyEmployees.Entities.DataTransferObjects;
using CompanyEmployees.Entities.Models;
using EmailSenderProject.Interfaces;
using EmailSenderProject.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using System.IdentityModel.Tokens.Jwt;

namespace CompanyEmployees.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly IMapper _mapper;
        private readonly JwtHandler _jwtHandler;
        private readonly IEmailSender _emailSender;

        public AccountController(UserManager<User> userManager, IMapper mapper, JwtHandler jwtHandler, IEmailSender emailSender)
        {
            _userManager = userManager;
            _mapper = mapper;
            _jwtHandler = jwtHandler;
            _emailSender = emailSender;
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterUser([FromBody] UserModel userModel)
        {
            if (userModel == null || !ModelState.IsValid)
                return BadRequest();

            var user = _mapper.Map<User>(userModel);
            var result = await _userManager.CreateAsync(user, userModel.Password);

            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description);

                return BadRequest(new RegisterResponse { Errors = errors });
            }

            await _userManager.AddToRoleAsync(user, "Viewer");

            return StatusCode(201);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel loginModel)
        {
            var user = await _userManager.FindByNameAsync(loginModel.Email);

            if (user == null || !await _userManager.CheckPasswordAsync(user, loginModel.Password))
            {
                return Unauthorized(new AuthResponse { ErrorMessage = "Invalid Authentication" });
            }

            var signingCredentials = _jwtHandler.GetSigningCredentials();
            var claims = await _jwtHandler.GetClaims(user);
            var tokenOptions = _jwtHandler.GenerateTokenOptions(signingCredentials, claims);
            var token = new JwtSecurityTokenHandler().WriteToken(tokenOptions);

            return Ok(new AuthResponse { IsAuthSuccessful = true, Token = token });
        }

        [HttpPost("ForgotPassword")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordVM model)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
                return BadRequest("Invalid request");

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var param = new Dictionary<string, string?>
            {
                {"token", token },
                {"email", model.Email}
            };

            var callback = QueryHelpers.AddQueryString(model.ClientURI, param);
            var message = new Message(new string[] { user.Email }, "Reset password token", callback);

            var result = await _emailSender.SendEmailAsync(message);

            return result ? Ok(result) : BadRequest(result);
        }
    }
}
