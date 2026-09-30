using API.Dtos;
using Core.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers.Admin
{
    [Route("api/admin/[controller]")]
    [ApiController]
    public class RolesController(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult> GetRoles()
        {
            var roles = await roleManager.Roles.Select(x => new
            {
                x.Id,
                x.Name,
                x.Description
            }).ToListAsync();
            return Ok(roles);
        }

        [HttpPost]
        public async Task<ActionResult> CreateRole(CreateRoleDto createRole)
        {
            var roleName = createRole.Name.Trim();
            if (string.IsNullOrWhiteSpace(roleName))
                return BadRequest(new
                {
                    message = "Role name is required"
                });

            var existRole = await roleManager.FindByNameAsync(roleName);
            if (existRole != null)
                return Conflict(new
                {
                    code = "DuplicateRoleName",
                    message = $"Role name '{roleName}' is already taken."
                });

            var role = new AppRole
            {
                Name = roleName,
                Description = createRole.Description?.Trim() ?? string.Empty
            };
            var result = await roleManager.CreateAsync(role);
            if (!result.Succeeded)
            {
                return BadRequest(new
                {
                    message = "Role creation failed.",
                    errors = result.Errors.Select(x => new
                    {
                        code = x.Code,
                        description = x.Description
                    })
                });
            }

            return CreatedAtAction(
            nameof(GetRole),
            new { id = role.Id },
            new
            {
                message = "Role created successfully.",
                roleId = role.Id,
                roleName = role.Name,
                description = role.Description
            });
        }

        [HttpGet("{id}")]
        public async Task<ActionResult> GetRole(string id)
        {
            var role = await roleManager.FindByIdAsync(id);
            if (role == null)
                return NotFound(new
                {
                    message = "Role not found."
                });

            return Ok(role);
        }


        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateRole(string id, CreateRoleDto roleDto)
        {
            var role = await roleManager.FindByIdAsync(id);
            if (role == null)
                return NotFound(new
                {
                    message = "Role not found."
                });

            role.Name = roleDto.Name;
            role.Description = roleDto.Description ?? string.Empty;

            // Check duplicate name
            var existingRole = await roleManager.FindByNameAsync(role.Name);

            if (existingRole != null && existingRole.Id != role.Id)
            {
                return Conflict(new
                {
                    code = "DuplicateRoleName",
                    message = $"Role name '{role.Name}' is already taken."
                });
            }


            var result = await roleManager.UpdateAsync(role);
            if (!result.Succeeded)
            {
                return BadRequest(new
                {
                    message = "Role update failed.",
                    errors = result.Errors.Select(x => new
                    {
                        code = x.Code,
                        description = x.Description
                    })
                });
            }

            return Ok(new
            {
                message = "Role updated successfully.",
                roleId = role.Id,
                roleName = role.Name,
                description = role.Description
            });
        }
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteRole(string id)
        {
            var role = await roleManager.FindByIdAsync(id);
            if (role == null)
            {
                return NotFound(new
                {
                    message = "Role not found."
                });
            }

            // Check whether role is assigned to users
            var usersInRole = await userManager.GetUsersInRoleAsync(role.Name!);
            if (usersInRole.Count > 0)
            {
                return Conflict(new
                {
                    code = "RoleInUse",
                    message = $"Role '{role.Name}' is assigned to {usersInRole.Count} user(s). Remove the role from users before deleting it."
                });
            }

            var result = await roleManager.DeleteAsync(role);
            if (!result.Succeeded) if (!result.Succeeded)
            {
                return BadRequest(new
                {
                    message = "Role deletion failed.",
                    errors = result.Errors.Select(x => new
                    {
                        code = x.Code,
                        description = x.Description
                    })
                });
            }

            return Ok(new
            {
                message = "Role deleted successfully.",
                roleId = role.Id,
                roleName = role.Name
            });
        }
    }
}
