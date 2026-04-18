namespace CodeImpact.Application.Reports.Dto;

public sealed record ExecutiveReportExportFileDto(
    byte[] Content,
    string ContentType,
    string FileName);
