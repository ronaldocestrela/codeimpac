using CodeImpact.Application.Admin.Dto;
using MediatR;

namespace CodeImpact.Application.Admin.Queries;

public sealed record GetAdminJobDetailQuery(Guid TaskId) : IRequest<AdminJobListItemDto?>;