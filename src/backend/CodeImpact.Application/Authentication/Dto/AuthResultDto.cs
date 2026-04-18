namespace CodeImpact.Application.Authentication.Dto
{
    public record AuthResultDto(string AccessToken, string RefreshToken, string TokenType = "Bearer");
}
