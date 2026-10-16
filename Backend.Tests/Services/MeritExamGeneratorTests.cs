using Backend.Services;

namespace Backend.Tests.Services;

public class MeritExamGeneratorTests
{
    [Fact]
    public void Build_WhenLevelIsProfesional_IncludesBehavioralCompetency()
    {
        var questions = MeritExamGenerator.Build(Sample());

        Assert.Contains(questions, question =>
            question.Section == "Comportamental" && question.Stem.Contains("Aporte técnico-profesional", StringComparison.Ordinal));
    }

    [Fact]
    public void Build_WhenManualHasAFunction_UsesThatFunctionAsCorrectOption()
    {
        const string function = "Desarrollar y mantener el sistema de información institucional.";
        var questions = MeritExamGenerator.Build(Sample() with { Functions = function });
        var functional = Assert.Single(questions, question => question.Section == "Funcional" && question.Topic == "Función esencial");

        Assert.Equal(function, functional.Options[functional.CorrectIndex]);
        Assert.Equal(4, functional.Options.Count);
    }

    [Fact]
    public void Build_WhenCallHasAnAxis_AsksForThatAxis()
    {
        var questions = MeritExamGenerator.Build(Sample() with
        {
            CallNumber = "4 de 2025",
            Axes = "Ingeniería de software"
        });

        var axis = Assert.Single(questions, question => question.Section == "Eje");
        Assert.Contains("4 de 2025", axis.Stem, StringComparison.Ordinal);
        Assert.Equal("Ingeniería de software", axis.Options[axis.CorrectIndex]);
    }

    [Theory]
    [InlineData("tecnico", "Técnico")]
    [InlineData("Profesional", "Profesional")]
    [InlineData("obra", null)]
    public void NormalizeLevel_AcceptsTheFourPublicLevels(string input, string? expected)
    {
        Assert.Equal(expected, MeritExamGenerator.NormalizeLevel(input));
    }

    private static MeritExamSource Sample()
    {
        return new MeritExamSource(
            "Profesional",
            "1 de 2026",
            "OPEC-12",
            "Profesional universitario",
            "Desarrollar soluciones de software para los procesos de la entidad y mantener los sistemas de información.",
            "Desarrollar y mantener el sistema de información institucional.",
            "Ingeniería de sistemas",
            null);
    }
}
