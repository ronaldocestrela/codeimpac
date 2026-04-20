using CodeImpact.Application.GitHub.Dto;
using MediatR;

namespace CodeImpact.Application.GitHub.Queries;

public sealed record GetContributionsQuery(
    Guid UserId,
    long? RepositoryId,
    string? OrganizationLogin,
    DateTime? From,
    DateTime? To,
    int Page,
    int PageSize) : IRequest<PagedContributionsDto>;
