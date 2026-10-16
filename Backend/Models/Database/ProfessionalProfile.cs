namespace Backend.Models.Database;

public class ProfessionalProfile
{
    public int ProfileId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Experience { get; set; } = string.Empty;

    public string Keywords { get; set; } = string.Empty;

    public bool UseForSearch { get; set; }

    public DateTime UpdatedOn { get; set; }
}
