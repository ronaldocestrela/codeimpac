using CodeImpact.Application.Reports.Dto;

namespace CodeImpact.Application.Reports;

public enum ExecutiveReportExportFormat
{
    Markdown,
    Pdf,
    Docx
}

public interface IExecutiveReportExportService
{
    ExecutiveReportExportFileDto Build(ExecutiveReportDto report, ExecutiveReportExportFormat format);
}
