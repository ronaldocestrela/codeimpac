using CodeImpact.Application.BackgroundJobs.Dto;
using MediatR;

namespace CodeImpact.Application.BackgroundJobs.Queries;

public sealed record GetBackgroundJobStatusQuery(Guid UserId, Guid TaskId) : IRequest<BackgroundJobStatusDto?>;
