namespace backend.Application.Common.Interfaces;

public interface IVectorStoreProviderSettings
{
    string ApiKey { get; set; }
    string Environment { get; set; }
    string IndexName { get; set; }
}
