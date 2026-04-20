using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using CodeImpact.Application.Authentication.Dto;
using CodeImpact.Infrastructure.Identity;
using CodeImpact.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using CodeImpact.Application.Common.Interfaces;
using CodeImpact.Domain.Common;
using CodeImpact.Domain.Entities;

namespace CodeImpact.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly CodeImpactDbContext _dbContext;
        private readonly ITokenService _tokenService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            CodeImpactDbContext dbContext,
            ITokenService tokenService,
            ILogger<AuthController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _dbContext = dbContext;
            _tokenService = tokenService;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] LoginRequestDto request)
        {
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser is not null)
            {
                return Conflict(new { Message = "Email already registered." });
            }

            var user = new AppUser
            {
                UserName = request.Email,
                Email = request.Email,
                FullName = request.Email.Split('@')[0]
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors.Select(e => e.Description));
            }

            var addToRoleResult = await _userManager.AddToRoleAsync(user, ApplicationRoles.Viewer);
            if (!addToRoleResult.Succeeded)
            {
                _logger.LogWarning(
                    "Could not assign default role {Role} to user {UserId}. Errors: {Errors}",
                    ApplicationRoles.Viewer,
                    user.Id,
                    string.Join(", ", addToRoleResult.Errors.Select(error => error.Description)));
            }

            var roles = await _userManager.GetRolesAsync(user);

            var accessToken = await _tokenService.CreateAccessTokenAsync(user.Id, user.Email ?? string.Empty, roles);
            var refreshToken = await _tokenService.CreateRefreshTokenAsync();

            var refreshTokenEntity = new RefreshToken(user.Id, refreshToken, DateTime.UtcNow.AddDays(30));
            _dbContext.RefreshTokens.Add(refreshTokenEntity);
            await _dbContext.SaveChangesAsync();

            return Created(string.Empty, new AuthResultDto(accessToken, refreshToken));
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user is null)
            {
                return Unauthorized();
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
            if (!result.Succeeded)
            {
                return Unauthorized();
            }

            var roles = await _userManager.GetRolesAsync(user);
            var accessToken = await _tokenService.CreateAccessTokenAsync(user.Id, user.Email ?? string.Empty, roles);
            var refreshToken = await _tokenService.CreateRefreshTokenAsync();

            var refreshTokenEntity = new RefreshToken(user.Id, refreshToken, DateTime.UtcNow.AddDays(30));
            _dbContext.RefreshTokens.Add(refreshTokenEntity);
            await _dbContext.SaveChangesAsync();

            return Ok(new AuthResultDto(accessToken, refreshToken));
        }

        [Authorize]
        [HttpGet("me")]
        public IActionResult Me()
        {
            var email = User.Claims.FirstOrDefault(c => c.Type == "email")?.Value;
            var subject = User.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value;
            var roles = User.Claims
                .Where(c => c.Type == ClaimTypes.Role)
                .Select(c => c.Value)
                .Distinct()
                .ToArray();

            return Ok(new { Email = email, Subject = subject, Roles = roles });
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] CodeImpact.Application.Authentication.Dto.RefreshRequestDto request)
        {
            var refreshToken = request.RefreshToken;
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                return BadRequest(new { Message = "Refresh token required." });
            }

            var refreshEntity = await _dbContext.RefreshTokens.FirstOrDefaultAsync(r => r.Token == refreshToken);
            if (refreshEntity is null || !refreshEntity.IsActive)
            {
                return Unauthorized();
            }

            var user = await _userManager.FindByIdAsync(refreshEntity.UserId.ToString());
            if (user is null)
            {
                return Unauthorized();
            }

            // Revoke current refresh token and issue a new pair
            refreshEntity.Revoke();
            _dbContext.RefreshTokens.Update(refreshEntity);

            var roles = await _userManager.GetRolesAsync(user);
            var newAccessToken = await _tokenService.CreateAccessTokenAsync(user.Id, user.Email ?? string.Empty, roles);
            var newRefreshToken = await _tokenService.CreateRefreshTokenAsync();

            var newRefreshEntity = new RefreshToken(user.Id, newRefreshToken, DateTime.UtcNow.AddDays(30));
            _dbContext.RefreshTokens.Add(newRefreshEntity);
            await _dbContext.SaveChangesAsync();

            return Ok(new AuthResultDto(newAccessToken, newRefreshToken));
        }
    }
}
