
using Microsoft.AspNetCore.Identity;

namespace Core.Entities;

public class AppRole:IdentityRole
{
 public string Description { get; set; } = string.Empty;
}
