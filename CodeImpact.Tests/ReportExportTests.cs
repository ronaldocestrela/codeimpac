using System.Text;
using CodeImpact.Application.Reports;
using CodeImpact.Application.Reports.Dto;
using CodeImpact.Infrastructure.Services;

namespace CodeImpact.Tests;

public class ReportExportTests
{
    private readonly ExecutiveReportDto _report = new(
        Guid.Parse("fdb89f46-6b68-4ce3-8f45-fce95af0f887"),
        new DateTime(2026, 04, 18, 12, 30, 0, DateTimeKind.Utc),
        new ExecutiveReportScopeDto(
            "john.doe@company.com",
            123,
            new DateTime(2026, 03, 01, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 03, 31, 23, 59, 59, DateTimeKind.Utc),
            new[] { "org/repo-a" }),
        new ExecutiveReportMetricsDto(
            12,
            2,
            6,
            5,
            4,
            18,
            1),
        "Resumo de teste",
        new[]
        {
            new ExecutiveReportHighlightDto(
                "Entrega consistente",
                "Ritmo estavel de entregas.",
                "Previsibilidade",
                new[] { "PR-101" })
        },
        new[]
        {
            new ExecutiveReportRiskDto(
                "Fila de review em crescimento",
                "Aumentar rotacao de revisores",
                new[] { "PR-202" })
        },
        new[]
        {
            new ExecutiveReportEvidenceDto(
                "PR-101",
                "pull_request",
                "org/repo-a",
                "#101",
                "john.doe",
                new DateTime(2026, 03, 15, 10, 0, 0, DateTimeKind.Utc),
                "approved",
                "https://github.com/org/repo-a/pull/101")
        });

    [Fact]
    public void Build_Markdown_ReturnsReadableMarkdownFile()
    {
        var service = new ExecutiveReportExportService();

        var file = service.Build(_report, ExecutiveReportExportFormat.Markdown);

        Assert.Equal("text/markdown; charset=utf-8", file.ContentType);
        Assert.EndsWith(".md", file.FileName);

        var content = Encoding.UTF8.GetString(file.Content);
        Assert.Contains("# Relatorio Executivo", content);
        Assert.Contains("Resumo de teste", content);
        Assert.Contains("PR-101", content);
    }

    [Fact]
    public void Build_Pdf_ReturnsPdfFileSignature()
    {
        var service = new ExecutiveReportExportService();

        var file = service.Build(_report, ExecutiveReportExportFormat.Pdf);

        Assert.Equal("application/pdf", file.ContentType);
        Assert.EndsWith(".pdf", file.FileName);
        Assert.True(file.Content.Length > 1000);

        var signature = Encoding.ASCII.GetString(file.Content.Take(4).ToArray());
        Assert.Equal("%PDF", signature);
    }

    [Fact]
    public void Build_Docx_ReturnsZipBasedDocxSignature()
    {
        var service = new ExecutiveReportExportService();

        var file = service.Build(_report, ExecutiveReportExportFormat.Docx);

        Assert.Equal("application/vnd.openxmlformats-officedocument.wordprocessingml.document", file.ContentType);
        Assert.EndsWith(".docx", file.FileName);
        Assert.True(file.Content.Length > 1000);
        Assert.Equal((byte)'P', file.Content[0]);
        Assert.Equal((byte)'K', file.Content[1]);
    }
}
