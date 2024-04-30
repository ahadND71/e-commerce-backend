using System.ComponentModel.DataAnnotations;

public class Category
{
  public Guid CategoryId { get; set; }

  [Required(ErrorMessage = "Category name is requierd")]
  [MaxLength(100), MinLength(2)]
  public string Name { get; set; }

  public string Slug { get; set; }

  [MaxLength(300)]
  public string Description { get; set; } = string.Empty;

  public DateTime CreatedAt { get; set; }
}
