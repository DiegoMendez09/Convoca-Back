using Backend.Services;

namespace Backend.Tests.Services;

public class ManualOpecExtractorTests
{
    private const string Manual = """
        IDENTIFICACIÓN DEL EMPLEO
        Denominación: Profesional Universitario
        Nivel: Profesional
        Grado: 2
        Número de OPEC: 220546
        PROPÓSITO PRINCIPAL
        Desarrollar los sistemas de información de la entidad.
        FUNCIONES ESENCIALES
        Diseñar y mantener las aplicaciones institucionales.
        Documentar los cambios de software.
        CONOCIMIENTOS BÁSICOS O ESENCIALES
        Ingeniería de sistemas
        Ley 909 de 2004
        IDENTIFICACIÓN DEL EMPLEO
        Denominación: Auxiliar administrativo
        Nivel: Asistencial
        Grado: 5
        Número de OPEC: 220547
        PROPÓSITO PRINCIPAL
        Apoyar el archivo de la dependencia.
        FUNCIONES ESENCIALES
        Organizar los expedientes físicos.
        """;

    [Fact]
    public void Extract_WhenOpecMatches_ReturnsOnlyThatEmployment()
    {
        var section = ManualOpecExtractor.Extract(Manual, "220546");

        Assert.NotNull(section);
        Assert.Equal("220546", section!.OpecCode);
        Assert.Equal("Profesional Universitario", section.JobTitle);
        Assert.Equal("Profesional", section.Level);
        Assert.Contains("sistemas de información", section.Purpose, StringComparison.Ordinal);
        Assert.Contains("aplicaciones institucionales", section.Functions, StringComparison.Ordinal);
        Assert.Contains("Ingeniería de sistemas", section.Knowledge, StringComparison.Ordinal);
        Assert.DoesNotContain("archivo de la dependencia", section.SectionText, StringComparison.Ordinal);
    }

    [Fact]
    public void Extract_WhenOpecIsMissing_ReturnsNull()
    {
        Assert.Null(ManualOpecExtractor.Extract(Manual, "999999"));
    }

    [Fact]
    public void Extract_WhenCodeHasOpecPrefix_StillMatches()
    {
        var section = ManualOpecExtractor.Extract(Manual, "OPEC 220547");

        Assert.NotNull(section);
        Assert.Equal("Auxiliar administrativo", section!.JobTitle);
        Assert.DoesNotContain("sistemas de información", section.Purpose, StringComparison.Ordinal);
    }

    [Fact]
    public void Extract_WhenCodeAlsoAppearsInAnIndex_UsesTheEmploymentBlock()
    {
        var manual = "Contenido\nOPEC 220546\nOPEC 220547\n" + Manual;
        var section = ManualOpecExtractor.Extract(manual, "220546");

        Assert.NotNull(section);
        Assert.Contains("sistemas de información", section!.Purpose, StringComparison.Ordinal);
        Assert.DoesNotContain("archivo de la dependencia", section.SectionText, StringComparison.Ordinal);
    }

    [Fact]
    public void Extract_WhenFichaCodePrecedesIdentification_ReturnsThatGrade()
    {
        const string manual = """
            CÓDIGO: OGDAD013
            I. IDENTIFICACIÓN DEL EMPLEO
            Nivel: Profesional
            Denominación del Empleo: Profesional Especializado
            Código: 2028
            Grado: 15
            III. PROPÓSITO PRINCIPAL
            Diseñar, desarrollar, probar, documentar y desplegar aplicaciones y sistemas de información.
            IV. DESCRIPCIÓN DE FUNCIONES ESENCIALES
            Desarrollar aplicaciones web y microservicios.
            V. CONOCIMIENTOS BÁSICOS O ESENCIALES
            Desarrollo de software.
            VI. COMPETENCIAS COMPORTAMENTALES
            Trabajo en equipo
            CÓDIGO: SGEN002A
            I. IDENTIFICACIÓN DEL EMPLEO
            Nivel: Profesional
            Denominación del Empleo: Profesional Especializado
            Código: 2028
            Grado: 13
            III. PROPÓSITO PRINCIPAL
            Apoyar el Sistema de Gestión Pública de la Secretaría General.
            IV. DESCRIPCIÓN DE FUNCIONES ESENCIALES
            Coordinar los planes de la secretaría.
            """;

        var section = ManualOpecExtractor.Extract(manual, "OGDAD013");

        Assert.NotNull(section);
        Assert.Equal("OGDAD013", section!.OpecCode);
        Assert.Equal("15", section.Grade);
        Assert.Equal("Profesional Especializado", section.JobTitle);
        Assert.Contains("sistemas de información", section.Purpose, StringComparison.Ordinal);
        Assert.Contains("microservicios", section.Functions, StringComparison.Ordinal);
        Assert.DoesNotContain("Secretaría General", section.Purpose, StringComparison.Ordinal);
    }
}
