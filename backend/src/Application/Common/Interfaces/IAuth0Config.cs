namespace backend.Application.Common.Interfaces
{
    public interface IAuth0Config
    {
        string? Domain { get; set; }
        string? Audience { get; set; }
        string? Namespace { get; set; }
    }
}
