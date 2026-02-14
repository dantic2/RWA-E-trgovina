using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebApp.Models;
using WebApp.Security;
using WebApp.ViewModels;

namespace WebApp.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly EcommerceDbContext _context;

        public ProfileController(EcommerceDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
            {
                return Unauthorized();
            }

            int userId = int.Parse(userIdClaim);

            var user = await _context.Users.FindAsync(userId);

            if (user == null)
            {
                return NotFound();
            }

            var viewModel = new ProfileViewModel
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Address = user.Address,
                Phone = user.Phone,
                IsAdmin = user.IsAdmin,
                CreatedAt = user.CreatedAt
            };

            return View(viewModel);
        }

        //  POST: profile/update ajax
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update([FromBody] ProfileViewModel model)
        {
            // validate user identity
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null || int.Parse(userIdClaim) != model.Id)
            {
                return Json(new { success = false, message = "Unauthorized" });
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return Json(new { success = false, message = "Validation failed", errors });
            }

            try
            {
                var user = await _context.Users.FindAsync(model.Id);

                if (user == null)
                {
                    return Json(new { success = false, message = "User not found" });
                }

                // check if email changed and is already taken
                if (user.Email != model.Email && await _context.Users.AnyAsync(u => u.Email == model.Email && u.Id != model.Id))
                {
                    return Json(new { success = false, message = "Email is already taken" });
                }

                // update user data
                user.Email = model.Email;
                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.Address = model.Address;
                user.Phone = model.Phone;

                _context.Update(user);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Profile updated successfully!  " });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Update failed: {ex.Message}" });
            }
        }


        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return Json(new { success = false, message = "Validation failed", errors });
            }

            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userIdClaim == null)
                {
                    return Json(new { success = false, message = "Unauthorized" });
                }

                int userId = int.Parse(userIdClaim);
                var user = await _context.Users.FindAsync(userId);

                if (user == null)
                {
                    return Json(new { success = false, message = "User not found" });
                }

                // verify current password
                var currentPasswordHash = PasswordHashProvider.GetHash(model.CurrentPassword, user.PwdSalt);

                if (currentPasswordHash != user.PwdHash)
                {
                    return Json(new { success = false, message = "Current password is incorrect" });
                }

                // check if new password is same as old
                var newPasswordHash = PasswordHashProvider.GetHash(model.NewPassword, user.PwdSalt);
                if (newPasswordHash == user.PwdHash)
                {
                    return Json(new { success = false, message = "New password must be different from current password" });
                }

                // generate new salt and hash for new password
                var newSalt = PasswordHashProvider.GetSalt();
                var newHash = PasswordHashProvider.GetHash(model.NewPassword, newSalt);

                // update user
                user.PwdSalt = newSalt;
                user.PwdHash = newHash;

                _context.Update(user);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Password changed successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Password change failed: {ex.Message}" });
            }
        }
    }
}