namespace backend.Application.Common.Models.AppSetting;

public class AppSettings
{
    // This class will be populated from appsettings.json
    // Add properties here that correspond to your appsettings.json structure
    // For example:
    // public string SomeSetting { get; set; } = null!;
    // public int AnotherSetting { get; set; } = 0;

    public AIChatSettings AIChatSettings { get; set; } = new AIChatSettings();
    public EmbeddingSettings EmbeddingSettings { get; set; } = new EmbeddingSettings();
    public VectorStoreSettings VectorStoreSettings { get; set; } = new VectorStoreSettings();
    public ConnectionStrings ConnectionStrings { get; set; } = new ConnectionStrings();
    public JwtSettings JwtSettings { get; set; } = new JwtSettings();
    public StorageSettings StorageSettings { get; set; } = new StorageSettings();
    public FaceDetectionSettings FaceDetectionService { get; set; } = new FaceDetectionSettings();
    public string CORS_ORIGINS { get; set; } = null!;
    public bool UseInMemoryDatabase { get; set; }
}
































