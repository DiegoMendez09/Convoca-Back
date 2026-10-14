using Backend.Services;

namespace Backend.Tests.Services;

public class SecopQueryBuilderTests
{
    [Fact]
    public void BuildWhere_WhenPublishedOnly_ExcludesDraftsAndRequiresPublicationDate()
    {
        var where = SecopQueryBuilder.BuildWhere(new ProcessSearchCriteria());

        Assert.Contains("fecha_de_publicacion_del IS NOT NULL", where, StringComparison.Ordinal);
        Assert.Contains("upper(estado_del_procedimiento) != 'BORRADOR'", where, StringComparison.Ordinal);
    }

    [Fact]
    public void BuildWhere_WhenKeywordContainsQuote_EscapesSoqlLiteral()
    {
        var where = SecopQueryBuilder.BuildWhere(new ProcessSearchCriteria
        {
            Keyword = "O'Brien",
            PublishedOnly = false
        });

        Assert.Contains("O''Brien", where, StringComparison.Ordinal);
        Assert.DoesNotContain("fecha_de_publicacion_del IS NOT NULL", where, StringComparison.Ordinal);
    }

    [Fact]
    public void BuildWhere_WhenDatesAndPriceAreSet_AddsBoundedClauses()
    {
        var where = SecopQueryBuilder.BuildWhere(new ProcessSearchCriteria
        {
            PublishedFrom = new DateTime(2026, 1, 2),
            PublishedTo = new DateTime(2026, 3, 4),
            MinBasePrice = 1000.5m,
            MaxBasePrice = 2000m,
            PublishedOnly = true
        });

        Assert.Contains("fecha_de_publicacion_del >= '2026-01-02T00:00:00'", where, StringComparison.Ordinal);
        Assert.Contains("fecha_de_publicacion_del <= '2026-03-04T23:59:59'", where, StringComparison.Ordinal);
        Assert.Contains("precio_base >= 1000.5", where, StringComparison.Ordinal);
        Assert.Contains("precio_base <= 2000", where, StringComparison.Ordinal);
    }

    [Fact]
    public void BuildProcessIdWhere_ReturnsExactMatch()
    {
        var where = SecopQueryBuilder.BuildProcessIdWhere("CO1.REQ.10");

        Assert.Equal("id_del_proceso = 'CO1.REQ.10'", where);
    }

    [Theory]
    [InlineData("CO1.REQ.10", true)]
    [InlineData("CO1.REQ.10'; drop", false)]
    [InlineData("", false)]
    public void IsSafeProcessId_RejectsInjectionCharacters(string processId, bool expected)
    {
        Assert.Equal(expected, SecopValueParser.IsSafeProcessId(processId));
    }
}
