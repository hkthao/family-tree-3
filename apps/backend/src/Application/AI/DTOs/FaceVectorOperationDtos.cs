namespace backend.Application.AI.Models;

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
    public Guid Id { get; set; }
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
    public List<string> PointIds { get; set; } = [];
}

// New DTOs for n8n webhook response
public class N8nFaceVectorPoint
{
    public string Id { get; set; } = null!;
    public float Score { get; set; }
    public Dictionary<string, object>? Payload { get; set; }
}

public class N8nFaceVectorResult
{
    public List<N8nFaceVectorPoint>? Points { get; set; }
}

public class N8nFaceVectorResponse
{
    public N8nFaceVectorResult? Result { get; set; }
    public string Status { get; set; } = null!;
    public double Time { get; set; }
}

