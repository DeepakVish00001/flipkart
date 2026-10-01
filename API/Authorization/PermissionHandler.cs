using Core.Entities;
using Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace API.Authorization;

public class PermissionHandler(
    UserManager<AppUser> userManager,
    StoreContext context) : AuthorizationHandler<PermissionRequirement>
{
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext authContext,
    PermissionRequirement requirement)
    {
        var userId = userManager.GetUserId(authContext.User);
        if (string.IsNullOrEmpty(userId)) return;

        var roleId = await context.UserRoles
                    .Where(x => x.UserId == userId)
                    .Select(x => x.RoleId)
                    .ToListAsync();

        if (!roleId.Any()) return;


        var hasPermission = await context.Set<IdentityRoleClaim<string>>()
            .AnyAsync(x =>
                roleId.Contains(x.RoleId) &&
                x.ClaimType == requirement.Permission &&
                x.ClaimValue == requirement.Permission);

        if (hasPermission)
        {
            authContext.Succeed(requirement);
        }


    }
}
