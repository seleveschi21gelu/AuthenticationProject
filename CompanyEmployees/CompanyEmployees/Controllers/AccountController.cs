﻿using AutoMapper;
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

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var param = new Dictionary<string, string?>
            {
                {"token",token },
                {"email", user.Email}
            };

            var callback = QueryHelpers.AddQueryString(userModel.ClientURI, param);

            var message = new Message(new string[] { user.Email }, "Email Confirmation token", callback, null);
            await _emailSender.SendEmailAsync(message);

            await _userManager.AddToRoleAsync(user, "Viewer");

            return StatusCode(201);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel loginModel)
        {
            var user = await _userManager.FindByNameAsync(loginModel.Email);
            if (user == null)
                return BadRequest("Invalid Request");

            if (!await _userManager.IsEmailConfirmedAsync(user))
            {
                return Unauthorized(new AuthResponse { ErrorMessage = "Email is not confirmed" });
            }

            if (!await _userManager.CheckPasswordAsync(user, loginModel.Password))
            {
                await _userManager.AccessFailedAsync(user);

                if (await _userManager.IsLockedOutAsync(user))
                {
                    var content = $@"Your account is locked out. To reset the password click this link: {loginModel.ClientURI}";
                    var message = new Message(new string[] { loginModel.Email },
                        "Locked out account information", content, null);

                    await _emailSender.SendEmailAsync(message);

                    return Unauthorized(new AuthResponse { ErrorMessage = "The account is locked out" });
                }

                return Unauthorized(new AuthResponse { ErrorMessage = "Invalid Authentication" });
            }
            else if (await _userManager.IsLockedOutAsync(user))
            {
                return Unauthorized(new AuthResponse { ErrorMessage = "The account is locked out" });
            }

            var signingCredentials = _jwtHandler.GetSigningCredentials();
            var claims = await _jwtHandler.GetClaims(user);
            var tokenOptions = _jwtHandler.GenerateTokenOptions(signingCredentials, claims);
            var token = new JwtSecurityTokenHandler().WriteToken(tokenOptions);

            var refreshToken = _jwtHandler.GenerateRefreshToken(user);

            await _userManager.ResetAccessFailedCountAsync(user);

            return Ok(new AuthResponse { IsAuthSuccessful = true, Token = token, RefreshToken = refreshToken });
        }

        [HttpPost("forgotPassword")]
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

        [HttpPost("ResetPassword")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordVM resetPassword)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var user = await _userManager.FindByEmailAsync(resetPassword.Email);
            if (user == null)
                return BadRequest("Invalid Request");

            var resetPasswordResult = await _userManager.ResetPasswordAsync(user, resetPassword.Token, resetPassword.Password);
            if (!resetPasswordResult.Succeeded)
            {
                var errors = resetPasswordResult.Errors.Select(e => e.Description);

                return BadRequest(new { Errors = errors });
            }

            await _userManager.SetLockoutEndDateAsync(user, new DateTime(2000, 1, 1));

            return Ok();
        }

        [HttpGet("emailConfirmation")]
        public async Task<IActionResult> EmailConfirmation([FromQuery] string email, [FromQuery] string token)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return BadRequest("Invalid Email Confirmation Request");
            }

            var confirmResult = await _userManager.ConfirmEmailAsync(user, token);
            if (!confirmResult.Succeeded)
            {
                return BadRequest("Invalid Email Confirmation Request");
            }

            return Ok();
        }

        [HttpPost("Refresh")]
        public IActionResult RefreshToken(TokenApiModel model)
        {
            if (model is null)
            {
                return BadRequest("Invalid client request");
            }

            string accessToken = model.AccessToken;
            string refreshToken = model.RefreshToken;

            var principal = _jwtHandler.GetPrincipalFromExpiredToken(accessToken);
            var username = principal.Identity.Name;

            var user = _jwtHandler.GetUser(username);

            if (user == null || user.RefreshToken != refreshToken || user.RefreshTokenExpiryTime <= DateTime.Now)
            {
                return BadRequest("Invalid client request");
            }

            var newAccessToken = new JwtSecurityTokenHandler().WriteToken(_jwtHandler.GenerateTokenOptions(_jwtHandler.GetSigningCredentials(),
                                                                          principal.Claims.ToList()));
            var newRefreshToken = _jwtHandler.GenerateRefreshToken(user);

            return Ok(new AuthResponse { Token = newAccessToken, RefreshToken = newRefreshToken });
        }
    }
}
