using System;
using System.Security.Claims;
using System.Threading.Tasks;
using CodeImpact.Application.BackgroundJobs.Commands;
using CodeImpact.Application.BackgroundJobs.Queries;
using CodeImpact.Application.Common.Interfaces;
using CodeImpact.Application.GitHub.Commands;
using CodeImpact.Application.GitHub.Dto;
using CodeImpact.Application.GitHub.Queries;
using CodeImpact.Application.Reports;
using CodeImpact.Application.Reports.Dto;
using CodeImpact.Application.Reports.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CodeImpact.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class GitHubController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IGitHubService _gitHubService;

        public GitHubController(IMediator mediator, IGitHubService gitHubService)
        {
            _mediator = mediator;
            _gitHubService = gitHubService;
        }

        [HttpGet("authorize-url")]
        public async Task<IActionResult> GetAuthorizeUrl()
        {
            var url = await _gitHubService.GetAuthorizationUrlAsync();
            return Ok(new { Url = url });
        }

        [HttpPost("callback")]
        public async Task<IActionResult> Callback([FromBody] LinkGitHubAccountRequestDto request)
        {
            var userId = GetUserId();
            if (userId == Guid.Empty)
            {
                return Unauthorized();
            }

            try
            {
                var account = await _mediator.Send(new LinkGitHubAccountCommand(userId, request.Code));
                return Ok(account);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { Message = ex.Message });
            }
        }

        [HttpGet("account")]
        public async Task<IActionResult> GetLinkedAccount()
        {
            var userId = GetUserId();
            if (userId == Guid.Empty)
            {
                return Unauthorized();
            }

            var account = await _mediator.Send(new GetGitHubAccountQuery(userId));
            if (account is null)
            {
                return NoContent();
            }

            return Ok(account);
        }

        [HttpGet("repositories")]
        public async Task<IActionResult> GetRepositories()
        {
            var userId = GetUserId();
            if (userId == Guid.Empty)
            {
                return Unauthorized();
            }

            var repositories = await _mediator.Send(new GetGitHubRepositoriesQuery(userId));
            return Ok(repositories);
        }

        [HttpGet("repositories/selected")]
        public async Task<IActionResult> GetSelectedRepositories()
        {
            var userId = GetUserId();
            if (userId == Guid.Empty)
            {
                return Unauthorized();
            }

            var repositories = await _mediator.Send(new GetSelectedGitHubRepositoriesQuery(userId));
            return Ok(repositories);
        }

        [HttpPost("repositories/select")]
        public async Task<IActionResult> SelectRepositories([FromBody] UpdateSelectedGitHubRepositoriesRequestDto request)
        {
            var userId = GetUserId();
            if (userId == Guid.Empty)
            {
                return Unauthorized();
            }

            try
            {
                await _mediator.Send(new UpdateSelectedGitHubRepositoriesCommand(userId, request.Repositories));
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpPost("repositories/{repositoryId}/sync")]
        public async Task<IActionResult> SyncRepository(long repositoryId)
        {
            var userId = GetUserId();
            if (userId == Guid.Empty)
            {
                return Unauthorized();
            }

            try
            {
                await _mediator.Send(new SyncGitHubRepositoryCommand(userId, repositoryId));
                return Accepted();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpGet("contributions")]
        public async Task<IActionResult> GetContributions([FromQuery] long? repositoryId, [FromQuery] DateTime? from, [FromQuery] DateTime? to)
        {
            var userId = GetUserId();
            if (userId == Guid.Empty)
            {
                return Unauthorized();
            }

            try
            {
                var contributions = await _mediator.Send(new GetContributionsQuery(userId, repositoryId, from, to));
                return Ok(contributions);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpPost("contributions/summary")]
        public async Task<IActionResult> GenerateContributionSummary([FromBody] GenerateContributionSummaryRequestDto request)
        {
            var userId = GetUserId();
            if (userId == Guid.Empty)
            {
                return Unauthorized();
            }

            try
            {
                var job = await _mediator.Send(new EnqueueContributionSummaryJobCommand(userId, request.RepositoryId, request.From, request.To));
                return Accepted(job);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpGet("contributions/commits/{contributionId:guid}")]
        public async Task<IActionResult> GetCommitContributionDetail(Guid contributionId)
        {
            var userId = GetUserId();
            if (userId == Guid.Empty)
            {
                return Unauthorized();
            }

            var contribution = await _mediator.Send(new GetCommitContributionDetailQuery(userId, contributionId));
            if (contribution is null)
            {
                return NotFound();
            }

            return Ok(contribution);
        }

        [HttpGet("contributions/pull-requests/{contributionId:guid}")]
        public async Task<IActionResult> GetPullRequestContributionDetail(Guid contributionId)
        {
            var userId = GetUserId();
            if (userId == Guid.Empty)
            {
                return Unauthorized();
            }

            var contribution = await _mediator.Send(new GetPullRequestContributionDetailQuery(userId, contributionId));
            if (contribution is null)
            {
                return NotFound();
            }

            return Ok(contribution);
        }

        [HttpPost("reports")]
        public async Task<IActionResult> GenerateExecutiveReport([FromBody] GenerateExecutiveReportRequestDto request)
        {
            var userId = GetUserId();
            if (userId == Guid.Empty)
            {
                return Unauthorized();
            }

            try
            {
                var job = await _mediator.Send(new EnqueueExecutiveReportJobCommand(userId, request.RepositoryId, request.From, request.To));
                return Accepted(job);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpGet("jobs/{taskId:guid}")]
        public async Task<IActionResult> GetBackgroundJobStatus(Guid taskId)
        {
            var userId = GetUserId();
            if (userId == Guid.Empty)
            {
                return Unauthorized();
            }

            var job = await _mediator.Send(new GetBackgroundJobStatusQuery(userId, taskId));
            if (job is null)
            {
                return NotFound();
            }

            return Ok(job);
        }

        [HttpGet("reports")]
        public async Task<IActionResult> GetExecutiveReports([FromQuery] long? repositoryId, [FromQuery] DateTime? from, [FromQuery] DateTime? to)
        {
            var userId = GetUserId();
            if (userId == Guid.Empty)
            {
                return Unauthorized();
            }

            var reports = await _mediator.Send(new GetExecutiveReportsQuery(userId, repositoryId, from, to));
            return Ok(reports);
        }

        [HttpGet("reports/{reportId:guid}")]
        public async Task<IActionResult> GetExecutiveReportDetail(Guid reportId)
        {
            var userId = GetUserId();
            if (userId == Guid.Empty)
            {
                return Unauthorized();
            }

            var report = await _mediator.Send(new GetExecutiveReportDetailQuery(userId, reportId));
            if (report is null)
            {
                return NotFound();
            }

            return Ok(report);
        }

        [HttpGet("reports/{reportId:guid}/export")]
        public async Task<IActionResult> ExportExecutiveReport(Guid reportId, [FromQuery] string format = "markdown")
        {
            var userId = GetUserId();
            if (userId == Guid.Empty)
            {
                return Unauthorized();
            }

            if (!TryParseExportFormat(format, out var exportFormat))
            {
                return BadRequest(new { Message = "Formato de exportacao invalido. Utilize: markdown, pdf ou docx." });
            }

            var exportFile = await _mediator.Send(new ExportExecutiveReportQuery(userId, reportId, exportFormat));
            if (exportFile is null)
            {
                return NotFound();
            }

            return File(exportFile.Content, exportFile.ContentType, exportFile.FileName);
        }

        private static bool TryParseExportFormat(string format, out ExecutiveReportExportFormat exportFormat)
        {
            exportFormat = ExecutiveReportExportFormat.Markdown;
            if (string.IsNullOrWhiteSpace(format))
            {
                return true;
            }

            var normalized = format.Trim().ToLowerInvariant();
            switch (normalized)
            {
                case "markdown":
                case "md":
                    exportFormat = ExecutiveReportExportFormat.Markdown;
                    return true;
                case "pdf":
                    exportFormat = ExecutiveReportExportFormat.Pdf;
                    return true;
                case "docx":
                    exportFormat = ExecutiveReportExportFormat.Docx;
                    return true;
                default:
                    return false;
            }
        }

        private Guid GetUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                        ?? User.FindFirst("sub")?.Value;

            return Guid.TryParse(claim, out var id) ? id : Guid.Empty;
        }
    }
}
