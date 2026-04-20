using CodeImpact.Application.BackgroundJobs.Dto;
using MediatR;

namespace CodeImpact.Application.BackgroundJobs.Commands;

public sealed record EnqueueExecutiveReportJobCommand(
    Guid UserId,
    long? RepositoryId,
    string? OrganizationLogin,
    DateTime? From,
    DateTime? To) : IRequest<BackgroundJobEnqueueDto>;
