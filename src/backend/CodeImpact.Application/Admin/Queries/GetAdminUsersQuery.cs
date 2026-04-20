using CodeImpact.Application.Admin.Dto;
using MediatR;

namespace CodeImpact.Application.Admin.Queries;

public sealed record GetAdminUsersQuery(
    string? Email,
    string? Status,
    int Page,
    int PageSize) : IRequest<AdminUserListDto>;