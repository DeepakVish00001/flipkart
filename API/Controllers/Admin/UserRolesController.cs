using API.Dtos;
using Core.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client.NativeInterop;

namespace API.Controllers.Admin
{
    [Route("api/admin/[controller]")]
    [ApiController]
    public class UserRolesController(
            UserManager<AppUser> userManager,
             RoleManager<AppRole> roleManager,
             SignInManager<AppUser> signInManager) : ControllerBase
    {
        [HttpGet("{userId}/roles")]
        public async Task<ActionResult> GetUserRoles(string userId)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound(new { message = "User not found!" });

            var roles = await userManager.GetRolesAsync(user);
            return Ok(new
            {
                userId = user.Id,
                userName = user.UserName,
                roles
            });
        }

        [HttpPost("{userId}/roles/{roleId}")]
        public async Task<ActionResult> AssignRole(string userId, string roleId)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound(new { message = "User not found!" });

            var role = await roleManager.FindByIdAsync(roleId);
            if (role == null)
                return NotFound(new { message = "Role not found!" });

            var hasRole = await userManager.IsInRoleAsync(user, role.Name!);
            if (hasRole)
                return Conflict(new
                {
                    code = "DuplicateRole",
                    message = $"User already has role '{role.Name}'."
                });

            var result = await userManager.AddToRoleAsync(user, role.Name!);
            if (!result.Succeeded)
                return BadRequest(new
                {
                    message = "Role assignment failed.",
                    errors = result.Errors.Select(x => new
                    {
                        code = x.Code,
                        description = x.Description
                    })
                });

            return Ok(new
            {
                message = "Role assigned successfully.",
                userId = user.Id,
                userName = user.UserName,
                roleId = role.Id,
                roleName = role.Name
            });

        }

        [HttpDelete("{userId}/roles/{roleId}")]
        public async Task<ActionResult> RemoveRole(string userId, string roleId)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound(new { message = "User not found!" });

            var role = await roleManager.FindByIdAsync(roleId);
            if (role == null)
                return NotFound(new { message = "Role not found!" });

            var hasRole = await userManager.IsInRoleAsync(user, role.Name!);
            if (!hasRole)
                return NotFound(new
                {
                    message = $"User does not have role '{role.Name}'."
                });

            var result = await userManager.RemoveFromRoleAsync(user, role.Name!);
            if (!result.Succeeded)
                return BadRequest(new
                {
                    message = "Role removal failed.",
                    errors = result.Errors.Select(x => new
                    {
                        code = x.Code,
                        description = x.Description
                    })
                });

            return Ok(new
            {
                message = "Role removed successfully.",
                userId = user.Id,
                userName = user.UserName,
                roleId = role.Id,
                roleName = role.Name
            });
        }

        [HttpPost("register")]
        public async Task<ActionResult> Register(RegisterDto registerDto)
        {
            var user = new AppUser{
                DisplayName= registerDto.DisplayName,
                Email=registerDto.Email,
                UserName=registerDto.Email
            };

            var result = await signInManager.UserManager.CreateAsync(user, registerDto.Password);
            if (!result.Succeeded)
            {
                foreach(var error in result.Errors)
                {
                    ModelState.AddModelError(error.Code, error.Description);
                }
                return ValidationProblem();
            }
            return Ok();
        }
    }
}
