using backend.Domain.Entities;

namespace backend.Domain.Events.MemberFaces;

/// <summary>
/// Sự kiện miền được kích hoạt khi một MemberFace bị xóa.
/// </summary>
    public class MemberFaceDeletedEvent : BaseEvent
    {
        public MemberFace MemberFace { get; }
        public string VectorDbId { get; } // Added

        public MemberFaceDeletedEvent(MemberFace memberFace)
        {
            MemberFace = memberFace;
            VectorDbId = memberFace.VectorDbId ?? string.Empty; // Assign VectorDbId
        }
    }