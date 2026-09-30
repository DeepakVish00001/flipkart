using API.Dtos;
using Core.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers.Admin
{
    [Route("api/admin/[controller]")]
    [ApiController]
    public class RolesController(RoleManager<AppRole> roleManager) : ControllerBase
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
            if (string.IsNullOrWhiteSpace(createRole.Name))
                return BadRequest("Role name is required");

            var existRole = await roleManager.FindByNameAsync(createRole.Name);
            if (existRole != null)
                return BadRequest("Role already exists.");
            var role = new AppRole
            {
                Name = createRole.Name,
                Description = createRole.Description ?? string.Empty
            };
            var result = await roleManager.CreateAsync(role);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            return Ok(new
            {
                message = "Role created successfully.",
                roleId = role.Id,
                roleName = role.Name
            });
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateRole(string id, CreateRoleDto roleDto)
        {
            var role = await roleManager.FindByIdAsync(id);
            if (role == null) return NotFound("Role not found");

            role.Name = roleDto.Name;
            role.Description = roleDto.Description ?? string.Empty;

            var result = await roleManager.UpdateAsync(role);
            if (!result.Succeeded) return BadRequest(result.Errors);

            return Ok(new
            {
                message = "Role updated successfully.",
                roleId = role.Id,
                roleName = role.Name
            });
        }
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteRole(string id)
        {
            var role = await roleManager.FindByIdAsync(id);
            if (role == null)
                return NotFound("Role not found.");

            var result = await roleManager.DeleteAsync(role);
            if (!result.Succeeded) return BadRequest(result.Errors);

            return Ok(new
            {
                message = "Role deleted successfully.",
                roleId = role.Id,
                roleName = role.Name
            });
        }
    }
}
