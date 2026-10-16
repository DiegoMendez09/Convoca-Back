using Backend.Services;

namespace Backend.Tests.Services;

public class ProfileSearchTextTests
{
    [Fact]
    public void Build_WhenKeywordsExist_UsesOnlyThoseTerms()
    {
        var text = ProfileSearchText.Build("Abogado", "Diez años en hospitales", "ingeniero civil, obra");

        Assert.Equal("ingeniero civil o obra", text);
    }

    [Fact]
    public void Build_WhenProfileIsEmpty_ReturnsNull()
    {
        Assert.Null(ProfileSearchText.Build("  ", "", null));
    }

    [Fact]
    public void Build_WhenOnlyExperienceExists_DropsCommonWords()
    {
        var text = ProfileSearchText.Build(null, "Experiencia laboral en interventoría de hospitales", null);

        Assert.Equal("interventoría o hospitales", text);
    }
}
