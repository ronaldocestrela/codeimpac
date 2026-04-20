using System.Security.Claims;
using CodeImpact.Application.Admin.Commands;
using CodeImpact.Application.Admin.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CodeImpact.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "AdminOnly")]
public class AdminController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok(new { Status = "ok", Scope = "admin" });
    }

    [HttpGet("jobs")]
    public async Task<IActionResult> GetJobs(
        [FromQuery] string? jobType,
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var jobs = await _mediator.Send(new GetAdminJobsQuery(jobType, status, page, pageSize));
        return Ok(jobs);
    }

    [HttpGet("jobs/{taskId:guid}")]
    public async Task<IActionResult> GetJobDetail(Guid taskId)
    {
        var job = await _mediator.Send(new GetAdminJobDetailQuery(taskId));
        return job is null ? NotFound() : Ok(job);
    }

    [HttpPost("jobs/{taskId:guid}/retry")]
    public async Task<IActionResult> RetryJob(Guid taskId)
    {
        var newTaskId = await _mediator.Send(new RetryAdminJobCommand(GetAdminUserId(), taskId, GetIpAddress()));
        return Accepted(new { TaskId = newTaskId });
    }

    [HttpGet("users")]
    public async Task<IActionResult> GetUsers(
        [FromQuery] string? email,
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var users = await _mediator.Send(new GetAdminUsersQuery(email, status, page, pageSize));
        return Ok(users);
    }

    [HttpGet("users/{userId:guid}")]
    public async Task<IActionResult> GetUserDetail(Guid userId)
    {
        var user = await _mediator.Send(new GetAdminUserDetailQuery(userId));
        return user is null ? NotFound() : Ok(user);
    }

    [HttpPatch("users/{userId:guid}/status")]
    public async Task<IActionResult> UpdateUserStatus(Guid userId, [FromBody] UpdateAdminUserStatusRequest request)
    {
        var updated = await _mediator.Send(new UpdateAdminUserStatusCommand(GetAdminUserId(), userId, request.Status, request.Reason, GetIpAddress()));
        return updated ? NoContent() : NotFound();
    }

    [HttpPatch("users/{userId:guid}/support-flags")]
    public async Task<IActionResult> UpdateUserSupportFlags(Guid userId, [FromBody] UpdateAdminUserSupportFlagsRequest request)
    {
        var updated = await _mediator.Send(new UpdateAdminUserSupportFlagsCommand(GetAdminUserId(), userId, request.SupportFlags, GetIpAddress()));
        return updated ? NoContent() : NotFound();
    }

    [HttpPost("users/{userId:guid}/revoke-github")]
    public async Task<IActionResult> RevokeGitHubAccess(Guid userId)
    {
        var revoked = await _mediator.Send(new RevokeAdminUserGitHubAccessCommand(GetAdminUserId(), userId, GetIpAddress()));
        return revoked ? NoContent() : NotFound();
    }

    [HttpPost("users/{userId:guid}/force-resync")]
    public async Task<IActionResult> ForceResync(Guid userId)
    {
        var syncedRepositories = await _mediator.Send(new ForceAdminUserResyncCommand(GetAdminUserId(), userId, GetIpAddress()));
        return Accepted(new { SyncedRepositories = syncedRepositories });
    }

    [HttpGet("users/{userId:guid}/subscription")]
    public async Task<IActionResult> GetUserSubscription(Guid userId)
    {
        var subscription = await _mediator.Send(new GetAdminUserSubscriptionQuery(userId));
        return Ok(subscription);
    }

    [HttpPatch("users/{userId:guid}/subscription")]
    public async Task<IActionResult> UpdateUserSubscription(Guid userId, [FromBody] UpdateAdminUserSubscriptionRequest request)
    {
        var updated = await _mediator.Send(new UpdateAdminUserSubscriptionCommand(
            GetAdminUserId(),
            userId,
            request.PlanId,
            request.Status,
            request.AutoRenew,
            request.CurrentPeriodEnd,
            request.BillingIssue,
            GetIpAddress()));

        return updated ? NoContent() : BadRequest(new { Message = "Não foi possível atualizar a assinatura do usuário." });
    }

    [HttpGet("audit-logs")]
    public async Task<IActionResult> GetAuditLogs(
        [FromQuery] string? action,
        [FromQuery] string? targetType,
        [FromQuery] Guid? adminUserId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var logs = await _mediator.Send(new GetAdminAuditLogsQuery(action, targetType, adminUserId, page, pageSize));
        return Ok(logs);
    }

    private Guid GetAdminUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                    ?? User.FindFirst("sub")?.Value;

        return Guid.TryParse(claim, out var id) ? id : Guid.Empty;
    }

    private string? GetIpAddress()
    {
        return HttpContext.Connection.RemoteIpAddress?.ToString();
    }

    public sealed record UpdateAdminUserStatusRequest(string Status, string? Reason);
    public sealed record UpdateAdminUserSupportFlagsRequest(string[] SupportFlags);
    public sealed record UpdateAdminUserSubscriptionRequest(Guid PlanId, string Status, bool AutoRenew, DateTime CurrentPeriodEnd, string? BillingIssue);
}