namespace CodeImpact.Application.Common.Interfaces
{
    public interface IGitHubTokenProtector
    {
        string Protect(string value);
        string Unprotect(string value);
    }
}
