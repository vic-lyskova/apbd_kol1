using System.ComponentModel.DataAnnotations;

namespace Kolokwium1.Models.DTOs;

public class NewBookWithGenresDTO
{
    [MaxLength(100)]
    [Required]
    public string Title { get; set; } = string.Empty;
    public List<int> Genres { get; set; } = new();
}