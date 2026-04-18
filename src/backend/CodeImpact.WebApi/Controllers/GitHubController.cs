using System;
using System.Security.Claims;
using System.Threading.Tasks;
using CodeImpact.Application.Common.Interfaces;
using CodeImpact.Application.GitHub.Commands;
using CodeImpact.Application.GitHub.Dto;
using CodeImpact.Application.GitHub.Queries;
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
                var summary = await _mediator.Send(new GenerateContributionSummaryQuery(userId, request.RepositoryId, request.From, request.To));
                return Ok(summary);
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

        private Guid GetUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                        ?? User.FindFirst("sub")?.Value;

            return Guid.TryParse(claim, out var id) ? id : Guid.Empty;
        }
    }
}
