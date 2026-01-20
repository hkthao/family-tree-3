using System;
using System.Collections.Generic;

namespace backend.Application.Knowledge.DTOs;

public class VectorData
{
    public string FamilyId { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Visibility { get; set; } = "public";
    public string Name { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
}
