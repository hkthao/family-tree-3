namespace backend.Application.AI.Models;

public class FaceVectorOperationDto
{
    public string ActionType { get; set; } = null!; // "upsert", "search", "delete"

    public List<float>? Vector { get; set; } // Face embedding vector for upsert/search
    public Dictionary<string, object>? Filter { get; set; } // { "memberId": "guid", "familyId": "guid", "faceId": "guid" }
    public Dictionary<string, object>? Payload { get; set; } // Metadata to store/filter on
    public int Limit { get; set; } = 10; // For search
    public float Threshold { get; set; } = 0.7f; // For search
    public List<string>? ReturnFields { get; set; } // For search, e.g., ["memberId", "faceId"]
}

public class FaceVectorSearchResultDto
{
    public string Id { get; set; } = null!; // ID of the found vector
    public float Score { get; set; }
    public Dictionary<string, object>? Payload { get; set; } // The payload/metadata associated with the vector
}

public class FaceVectorOperationResultDto
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public List<FaceVectorSearchResultDto>? SearchResults { get; set; } // For search operations
    public int? AffectedCount { get; set; } // For upsert/delete operations
}

public class UpsertFaceVectorOperationDto
{
    public List<float> Vector { get; set; } = new List<float>();
    public Dictionary<string, object> Payload { get; set; } = new Dictionary<string, object>();
    public Dictionary<string, object>? Filter { get; set; } // Optional, for updating existing vectors by filter
}

public class SearchFaceVectorOperationDto
{
    public List<float> Vector { get; set; } = new List<float>();
    public Dictionary<string, object>? Filter { get; set; }
    public int Limit { get; set; } = 10;
    public float Threshold { get; set; } = 0.7f;
    public List<string>? ReturnFields { get; set; }
}

public class DeleteFaceVectorOperationDto
{
    public Dictionary<string, object> Filter { get; set; } = new Dictionary<string, object>();
}
