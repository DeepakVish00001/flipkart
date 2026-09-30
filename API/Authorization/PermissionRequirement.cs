using Microsoft.AspNetCore.Authorization;

namespace API.Authorization;

public class PermissionRequirement(string Permission) : IAuthorizationRequirement
{
    public string Permission { get; } = Permission;
}
