using CodeImpact.Application.Admin.Dto;
using MediatR;

namespace CodeImpact.Application.Admin.Queries;

public sealed record GetAdminUserDetailQuery(Guid UserId) : IRequest<AdminUserDetailDto?>;