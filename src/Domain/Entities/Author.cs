using System.ComponentModel.DataAnnotations;

namespace Domain.Entities;

public class Author : Entity
{
    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string Email { get; set; } = string.Empty;

    public IEnumerable<Post> Posts { get; set; } = new List<Post>();
}
