using System.ComponentModel.DataAnnotations;

namespace API.Dtos;

public class CreateRoleDto
{
    [Required]
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}
