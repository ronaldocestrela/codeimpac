using CodeImpact.Application.BackgroundJobs.Dto;
using MediatR;

namespace CodeImpact.Application.BackgroundJobs.Commands;

public sealed record EnqueueExecutiveReportJobCommand(
    Guid UserId,
    long? RepositoryId,
    DateTime? From,
    DateTime? To) : IRequest<BackgroundJobEnqueueDto>;
