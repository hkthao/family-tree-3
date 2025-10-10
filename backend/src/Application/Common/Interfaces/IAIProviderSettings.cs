namespace backend.Application.Common.Interfaces;

public interface IAIProviderSettings
{
    string ApiKey { get; set; }
    string Model { get; set; }
}
