using CodeImpact.Application.Admin.Dto;
using MediatR;

namespace CodeImpact.Application.Admin.Queries;

public sealed record GetAdminJobsQuery(
    string? JobType,
    string? Status,
    int Page,
    int PageSize) : IRequest<AdminJobListDto>;