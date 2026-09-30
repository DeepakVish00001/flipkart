using System.Security.Claims;
using Core.Entities;
using Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers.Admin
{
    [Route("api/admin/roles")]
    [ApiController]
    public class RolePermissionsController(RoleManager<AppRole> roleManager, StoreContext context) : ControllerBase
    {
        [HttpGet("{roleId}/permissions")]
        public async Task<ActionResult> GetPermissions(string roleId)
        {
            var role = await roleManager.FindByIdAsync(roleId);

            if (role == null)
                return NotFound(new { message = "Role not found!" });

            var claims = await roleManager.GetClaimsAsync(role);

            var permissions = claims
                .Where(x => x.Type == "Permission")
                .Select(x => x.Value)
                .ToList();
            return Ok(permissions);
        }

        [HttpPost("{roleId}/permissions")]
        public async Task<ActionResult> CreatePermission(string roleId, [FromBody] string permission)
        {
            if (string.IsNullOrWhiteSpace(permission))
                return BadRequest(new { message = "Permission is required." });

            var role = await roleManager.FindByIdAsync(roleId);
            if (role == null)
                return NotFound(new { message = "Role not found" });

            var claims = await roleManager.GetClaimsAsync(role);

            var alreadyExists = claims.Any(x =>
                x.Type == "Permission" &&
                x.Value == permission);
            if (alreadyExists)
                return Conflict(new
                {
                    code = "DuplicatePermission",
                    message = $"Permission name '{permission}' is already taken."
                });

            var result = await roleManager.AddClaimAsync(role,
                new Claim("Permission", permission));
            if (!result.Succeeded)
                return BadRequest(new
                {
                    message = "Permission creation failed.",
                    errors = result.Errors.Select(x => new
                    {
                        code = x.Code,
                        description = x.Description
                    })
                });

            return Ok(new
            {
                message = "Permission created successfully.",
                role = role.Name,
                permission
            });
        }


        [HttpDelete("{roleId}/permissions/{permissionId:int}")]
        public async Task<ActionResult> DeletePermission(string roleId, int permissionId)
        {
            var role = await roleManager.FindByIdAsync(roleId);
            if (role == null)
                return NotFound(new { message = "Role not found." });

            var permission = await context.Set<IdentityRoleClaim<string>>()
                    .FirstOrDefaultAsync(x =>
                        x.Id == permissionId &&
                        x.RoleId == roleId &&
                        x.ClaimType == "Permission");
            if (permission == null)
                return NotFound(new
                {
                    message = "Permission not found for this role."
                });

            context.Set<IdentityRoleClaim<string>>()
                    .Remove(permission);

            await context.SaveChangesAsync();

            return Ok(new
            {
                message = "Permission deleted successfully.",
                roleId = role.Id,
                roleName = role.Name,
                permissionId = permission.Id,
                permission = permission.ClaimValue
            });


        }
    }
}
