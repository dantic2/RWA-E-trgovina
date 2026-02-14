using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAPI.DTOs;
using WebAPI.Models;
using WebAPI.Security;
using WebAPI.Services;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly EcommerceDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogService _logService;

        public AuthController(EcommerceDbContext context, IConfiguration configuration, ILogService logService)
        {
            _context = context;
            _configuration = configuration;
            _logService = logService;
        }

        [HttpPost("[action]")]
        public async Task<ActionResult<UserDto>> Register([FromBody] UserDto userDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    await _logService.LogWarning("User registration failed: Invalid model state");
                    return BadRequest(ModelState);
                }

                var existingUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Username == userDto.Username);

                if (existingUser != null)
                {
                    await _logService.LogWarning($"User registration failed: Username '{userDto.Username}' already exists");
                    return BadRequest($"Username '{userDto.Username}' already exists");
                }

                var existingEmail = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == userDto.Email);

                if (existingEmail != null)
                {
                    await _logService.LogWarning($"User registration failed: Email '{userDto.Email}' already exists");
                    return BadRequest($"Email '{userDto.Email}' already exists");
                }

                var salt = PasswordHashProvider.GetSalt();
                var hash = PasswordHashProvider.GetHash(userDto.Password, salt);

                var newUser = new User
                {
                    Username = userDto.Username,
                    PwdHash = hash,
                    PwdSalt = salt,
                    FirstName = userDto.FirstName,
                    LastName = userDto.LastName,
                    Address = userDto.Address,
                    Email = userDto.Email,
                    Phone = userDto.Phone,
                    IsAdmin = false, // default: obicni user
                    CreatedAt = DateTime.UtcNow
                };

                _context.Users.Add(newUser);
                await _context.SaveChangesAsync();

                await _logService.LogInfo($"User registered: '{newUser.Username}' (id={newUser.Id}, email={newUser.Email})");

                userDto.Id = newUser.Id;
                userDto.Password = null!;

                return CreatedAtAction(nameof(Register), new { id = newUser.Id }, userDto);
            }
            catch (Exception ex)
            {
                await _logService.LogError($"User registration failed for '{userDto.Username}': {ex.Message}");    
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("[action]")]
        public async Task<ActionResult<string>> Login([FromBody] LoginDto loginDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    await _logService.LogWarning("Login failed: Invalid model state");
                    return BadRequest(ModelState);
                }

                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Username == loginDto.Username);

                if (user == null)
                {
                    await _logService.LogWarning($"Login failed: Username '{loginDto.Username}' not found");
                    return BadRequest("Invalid username or password");
                }

                var hash = PasswordHashProvider.GetHash(loginDto.Password, user.PwdSalt);

                if (hash != user.PwdHash)
                {
                    await _logService.LogWarning($"Login failed: Invalid password for user '{loginDto.Username}'");
                    return BadRequest("Invalid username or password");
                }

                var secureKey = _configuration["JWT:SecureKey"];
                var issuer = _configuration["JWT:Issuer"];
                var audience = _configuration["JWT:Audience"];
                var expiration = int.Parse(_configuration["JWT:ExpirationMinutes"] ?? "120");

                var role = user.IsAdmin ? "Admin" : "User";

                var token = JwtTokenProvider.CreateToken(
                    secureKey!,
                    issuer!,
                    audience!,
                    expiration,
                    user.Username,
                    role);

                await _logService.LogInfo($"User logged in: '{user.Username}' (role={role})");

                return Ok(new { token, username = user.Username, role });
            }
            catch (Exception ex)
            {
                await _logService.LogError($"Login failed for '{loginDto.Username}': {ex.Message}");
                return StatusCode(500, ex.Message);
            }
        }

        [Authorize]
        [HttpPost("[action]")]
        public async Task<ActionResult> ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    await _logService.LogWarning("Change password failed: Invalid model state");
                    return BadRequest(ModelState);
                }

                var username = User.Identity?.Name;

                if (string.IsNullOrEmpty(username))
                {
                    await _logService.LogWarning("Change password failed: User not authenticated");
                    return Unauthorized("User not authenticated");
                }

                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Username == username);

                if (user == null)
                {
                    await _logService.LogWarning($"Change password failed: User '{username}' not found");
                    return NotFound("User not found");
                }

                var oldHash = PasswordHashProvider.GetHash(changePasswordDto.OldPassword, user.PwdSalt);

                if (oldHash != user.PwdHash)
                {
                    await _logService.LogWarning($"Change password failed:  Incorrect old password for user '{username}'");
                    return BadRequest("Old password is incorrect");
                }

                var newSalt = PasswordHashProvider.GetSalt();
                var newHash = PasswordHashProvider.GetHash(changePasswordDto.NewPassword, newSalt);

                user.PwdHash = newHash;
                user.PwdSalt = newSalt;

                await _context.SaveChangesAsync();

                // Log
                await _logService.LogInfo($"Password changed for user: '{username}'");

                return Ok("Password changed successfully");
            }
            catch (Exception ex)
            {
                await _logService.LogError($"Change password failed:  {ex.Message}");
                return StatusCode(500, ex.Message);
            }
        }
    }
}