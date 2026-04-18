using System.Globalization;
using System.Text;
using CodeImpact.Application.Reports;
using CodeImpact.Application.Reports.Dto;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using OpenXmlDocument = DocumentFormat.OpenXml.Wordprocessing.Document;

namespace CodeImpact.Infrastructure.Services
{
    public sealed class ExecutiveReportExportService : IExecutiveReportExportService
    {
        private const string MarkdownContentType = "text/markdown; charset=utf-8";
        private const string PdfContentType = "application/pdf";
        private const string DocxContentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";

        static ExecutiveReportExportService()
        {
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public ExecutiveReportExportFileDto Build(ExecutiveReportDto report, ExecutiveReportExportFormat format)
        {
            return format switch
            {
                ExecutiveReportExportFormat.Markdown => BuildMarkdownExport(report),
                ExecutiveReportExportFormat.Pdf => BuildPdfExport(report),
                ExecutiveReportExportFormat.Docx => BuildDocxExport(report),
                _ => throw new InvalidOperationException($"Unsupported export format: {format}")
            };
        }

        private static ExecutiveReportExportFileDto BuildMarkdownExport(ExecutiveReportDto report)
        {
            var markdown = BuildMarkdown(report);
            var content = Encoding.UTF8.GetBytes(markdown);
            return new ExecutiveReportExportFileDto(content, MarkdownContentType, BuildFileName(report, "md"));
        }

        private static ExecutiveReportExportFileDto BuildPdfExport(ExecutiveReportDto report)
        {
            var bytes = QuestPDF.Fluent.Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(28);
                    page.DefaultTextStyle(style => style.FontSize(11));

                    page.Header().Text("Relatorio Executivo - CodeImpact").Bold().FontSize(16);
                    page.Content().PaddingVertical(8).Column(column =>
                    {
                        column.Spacing(8);
                        AddMetaBlock(column, report);
                        AddMetricsBlock(column, report);
                        AddTextBlock(column, "Resumo Executivo", report.ExecutiveSummary);
                        AddHighlightsBlock(column, report);
                        AddRisksBlock(column, report);
                        AddEvidenceBlock(column, report);
                    });

                    page.Footer().AlignRight().Text(text =>
                    {
                        text.Span("Pagina ");
                        text.CurrentPageNumber();
                        text.Span(" de ");
                        text.TotalPages();
                    });
                });
            }).GeneratePdf();

            return new ExecutiveReportExportFileDto(bytes, PdfContentType, BuildFileName(report, "pdf"));
        }

        private static ExecutiveReportExportFileDto BuildDocxExport(ExecutiveReportDto report)
        {
            using var stream = new MemoryStream();
            using (var wordDoc = WordprocessingDocument.Create(stream, WordprocessingDocumentType.Document, true))
            {
                var mainPart = wordDoc.AddMainDocumentPart();
                mainPart.Document = new OpenXmlDocument(new Body());
                var body = mainPart.Document.Body;

                if (body is null)
                {
                    throw new InvalidOperationException("Could not initialize DOCX document body.");
                }

                body.Append(CreateHeadingParagraph("Relatorio Executivo", 28));
                body.Append(CreateParagraph($"Gerado em: {ToPtBrDateTime(report.GeneratedAt)}"));
                body.Append(CreateParagraph($"Escopo: {report.Scope.DeveloperScope}"));
                body.Append(CreateParagraph($"Repositorios: {string.Join(", ", report.Scope.Repositories)}"));
                body.Append(CreateParagraph($"Periodo: {ToOptionalDate(report.Scope.From)} ate {ToOptionalDate(report.Scope.To)}"));
                body.Append(CreateParagraph(string.Empty));

                body.Append(CreateHeadingParagraph("Metricas", 24));
                body.Append(CreateParagraph($"Commits: {report.Metrics.CommitCount}"));
                body.Append(CreateParagraph($"PRs abertos: {report.Metrics.PullRequestOpenCount}"));
                body.Append(CreateParagraph($"PRs fechados: {report.Metrics.PullRequestClosedCount}"));
                body.Append(CreateParagraph($"PRs mergeados: {report.Metrics.PullRequestMergedCount}"));
                body.Append(CreateParagraph($"PRs aprovados: {report.Metrics.PullRequestApprovedCount}"));
                body.Append(CreateParagraph($"Lead time medio (h): {report.Metrics.AverageMergeLeadTimeHours?.ToString(CultureInfo.InvariantCulture) ?? "-"}"));
                body.Append(CreateParagraph(string.Empty));

                body.Append(CreateHeadingParagraph("Resumo Executivo", 24));
                body.Append(CreateParagraph(report.ExecutiveSummary));
                body.Append(CreateParagraph(string.Empty));

                body.Append(CreateHeadingParagraph("Highlights", 24));
                if (report.Highlights.Count == 0)
                {
                    body.Append(CreateParagraph("- Nenhum highlight disponivel."));
                }
                else
                {
                    foreach (var highlight in report.Highlights)
                    {
                        body.Append(CreateParagraph($"- {highlight.Title}: {highlight.Insight}"));
                        body.Append(CreateParagraph($"  Impacto: {highlight.Impact}"));
                        body.Append(CreateParagraph($"  Evidencias: {string.Join(", ", highlight.EvidenceIds)}"));
                    }
                }

                body.Append(CreateParagraph(string.Empty));
                body.Append(CreateHeadingParagraph("Riscos e Proximos Passos", 24));
                if (report.Risks.Count == 0)
                {
                    body.Append(CreateParagraph("- Nenhum risco disponivel."));
                }
                else
                {
                    foreach (var risk in report.Risks)
                    {
                        body.Append(CreateParagraph($"- Risco: {risk.Risk}"));
                        body.Append(CreateParagraph($"  Recomendacao: {risk.Recommendation}"));
                        body.Append(CreateParagraph($"  Evidencias: {string.Join(", ", risk.EvidenceIds)}"));
                    }
                }

                body.Append(CreateParagraph(string.Empty));
                body.Append(CreateHeadingParagraph("Evidencias", 24));
                if (report.Evidence.Count == 0)
                {
                    body.Append(CreateParagraph("- Nenhuma evidencia registrada."));
                }
                else
                {
                    foreach (var evidence in report.Evidence)
                    {
                        body.Append(CreateParagraph(
                            $"- [{evidence.EvidenceId}] {evidence.EvidenceType} | {evidence.RepositoryFullName} | {evidence.Author} | {ToPtBrDateTime(evidence.OccurredAt)} | {evidence.Status} | {evidence.Url}"));
                    }
                }

                mainPart.Document.Save();
            }

            return new ExecutiveReportExportFileDto(stream.ToArray(), DocxContentType, BuildFileName(report, "docx"));
        }

        private static string BuildMarkdown(ExecutiveReportDto report)
        {
            var highlights = report.Highlights.Count > 0
                ? string.Join("\n", report.Highlights.Select(item =>
                    $"- **{item.Title}**: {item.Insight}\n  - Impacto: {item.Impact}\n  - Evidencias: {string.Join(", ", item.EvidenceIds)}"))
                : "- Nenhum highlight disponivel.";

            var risks = report.Risks.Count > 0
                ? string.Join("\n", report.Risks.Select(item =>
                    $"- **Risco**: {item.Risk}\n  - Recomendacao: {item.Recommendation}\n  - Evidencias: {string.Join(", ", item.EvidenceIds)}"))
                : "- Nenhum risco disponivel.";

            var evidence = report.Evidence.Count > 0
                ? string.Join("\n", report.Evidence.Select(item =>
                    $"- [{item.EvidenceId}] {item.EvidenceType} | {item.RepositoryFullName} | {item.Author} | {ToPtBrDateTime(item.OccurredAt)} | {item.Status} | {item.Url}"))
                : "Nenhuma evidencia registrada.";

            return string.Join("\n", new[]
            {
                "# Relatorio Executivo",
                string.Empty,
                $"- ID: {report.Id}",
                $"- Gerado em: {ToPtBrDateTime(report.GeneratedAt)}",
                $"- Escopo: {report.Scope.DeveloperScope}",
                $"- Repositorios: {string.Join(", ", report.Scope.Repositories)}",
                $"- Periodo: {ToOptionalDate(report.Scope.From)} ate {ToOptionalDate(report.Scope.To)}",
                string.Empty,
                "## Metricas",
                string.Empty,
                $"- Commits: {report.Metrics.CommitCount}",
                $"- PRs abertos: {report.Metrics.PullRequestOpenCount}",
                $"- PRs fechados: {report.Metrics.PullRequestClosedCount}",
                $"- PRs mergeados: {report.Metrics.PullRequestMergedCount}",
                $"- PRs aprovados: {report.Metrics.PullRequestApprovedCount}",
                $"- Lead time medio (h): {report.Metrics.AverageMergeLeadTimeHours?.ToString(CultureInfo.InvariantCulture) ?? "-"}",
                string.Empty,
                "## Resumo Executivo",
                string.Empty,
                report.ExecutiveSummary,
                string.Empty,
                "## Highlights",
                string.Empty,
                highlights,
                string.Empty,
                "## Riscos e Proximos Passos",
                string.Empty,
                risks,
                string.Empty,
                "## Evidencias",
                string.Empty,
                evidence
            });
        }

        private static void AddMetaBlock(ColumnDescriptor column, ExecutiveReportDto report)
        {
            column.Item().Text($"Gerado em: {ToPtBrDateTime(report.GeneratedAt)}");
            column.Item().Text($"Escopo: {report.Scope.DeveloperScope}");
            column.Item().Text($"Repositorios: {string.Join(", ", report.Scope.Repositories)}");
            column.Item().Text($"Periodo: {ToOptionalDate(report.Scope.From)} ate {ToOptionalDate(report.Scope.To)}");
        }

        private static void AddMetricsBlock(ColumnDescriptor column, ExecutiveReportDto report)
        {
            column.Item().PaddingTop(6).Text("Metricas").Bold().FontSize(13);
            column.Item().Text($"Commits: {report.Metrics.CommitCount}");
            column.Item().Text($"PRs abertos: {report.Metrics.PullRequestOpenCount}");
            column.Item().Text($"PRs fechados: {report.Metrics.PullRequestClosedCount}");
            column.Item().Text($"PRs mergeados: {report.Metrics.PullRequestMergedCount}");
            column.Item().Text($"PRs aprovados: {report.Metrics.PullRequestApprovedCount}");
            column.Item().Text($"Lead time medio (h): {report.Metrics.AverageMergeLeadTimeHours?.ToString(CultureInfo.InvariantCulture) ?? "-"}");
        }

        private static void AddTextBlock(ColumnDescriptor column, string title, string content)
        {
            column.Item().PaddingTop(6).Text(title).Bold().FontSize(13);
            column.Item().Text(string.IsNullOrWhiteSpace(content) ? "Sem conteudo." : content);
        }

        private static void AddHighlightsBlock(ColumnDescriptor column, ExecutiveReportDto report)
        {
            column.Item().PaddingTop(6).Text("Highlights").Bold().FontSize(13);
            if (report.Highlights.Count == 0)
            {
                column.Item().Text("- Nenhum highlight disponivel.");
                return;
            }

            foreach (var item in report.Highlights)
            {
                column.Item().Text($"- {item.Title}: {item.Insight}");
                column.Item().Text($"  Impacto: {item.Impact}");
                column.Item().Text($"  Evidencias: {string.Join(", ", item.EvidenceIds)}");
            }
        }

        private static void AddRisksBlock(ColumnDescriptor column, ExecutiveReportDto report)
        {
            column.Item().PaddingTop(6).Text("Riscos e Proximos Passos").Bold().FontSize(13);
            if (report.Risks.Count == 0)
            {
                column.Item().Text("- Nenhum risco disponivel.");
                return;
            }

            foreach (var item in report.Risks)
            {
                column.Item().Text($"- Risco: {item.Risk}");
                column.Item().Text($"  Recomendacao: {item.Recommendation}");
                column.Item().Text($"  Evidencias: {string.Join(", ", item.EvidenceIds)}");
            }
        }

        private static void AddEvidenceBlock(ColumnDescriptor column, ExecutiveReportDto report)
        {
            column.Item().PaddingTop(6).Text("Evidencias").Bold().FontSize(13);
            if (report.Evidence.Count == 0)
            {
                column.Item().Text("- Nenhuma evidencia registrada.");
                return;
            }

            foreach (var item in report.Evidence)
            {
                column.Item().Text($"- [{item.EvidenceId}] {item.EvidenceType} | {item.RepositoryFullName} | {item.Author} | {ToPtBrDateTime(item.OccurredAt)} | {item.Status} | {item.Url}");
            }
        }

        private static Paragraph CreateHeadingParagraph(string text, int size)
        {
            return new Paragraph(
                new ParagraphProperties(
                    new SpacingBetweenLines { After = "120" }),
                new Run(
                    new RunProperties(
                        new Bold(),
                        new FontSize { Val = size.ToString(CultureInfo.InvariantCulture) }),
                    new Text(text)));
        }

        private static Paragraph CreateParagraph(string text)
        {
            return new Paragraph(
                new ParagraphProperties(new SpacingBetweenLines { After = "80" }),
                new Run(new Text(text) { Space = SpaceProcessingModeValues.Preserve }));
        }

        private static string BuildFileName(ExecutiveReportDto report, string extension)
        {
            var generatedAt = report.GeneratedAt
                .ToString("yyyy-MM-ddTHH-mm-ssZ", CultureInfo.InvariantCulture);

            return $"codeimpact-report-{generatedAt}.{extension}";
        }

        private static string ToPtBrDateTime(DateTime value)
        {
            return value.ToLocalTime().ToString("g", CultureInfo.GetCultureInfo("pt-BR"));
        }

        private static string ToOptionalDate(DateTime? value)
        {
            return value.HasValue
                ? value.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
                : "-";
        }
    }
}
