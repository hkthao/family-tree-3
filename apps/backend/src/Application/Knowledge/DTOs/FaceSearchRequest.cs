using System;
using System.Collections.Generic;

namespace backend.Application.Knowledge.DTOs;

public class FaceSearchRequest
{
    public Guid FamilyId { get; set; }
    public List<double>? QueryEmbedding { get; set; }
    public Guid? MemberId { get; set; }
    public int TopK { get; set; }
}
