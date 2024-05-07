using System.ComponentModel.DataAnnotations;

namespace Kolokwium1.Models.DTOs;

public class BookWithGenresDTO
{
    [Required]
    public int Id { get; set; }
    [MaxLength(100)]
    [Required]
    public string Title { get; set; } = string.Empty;
    public List<string> Genres { get; set; } = new ();
}