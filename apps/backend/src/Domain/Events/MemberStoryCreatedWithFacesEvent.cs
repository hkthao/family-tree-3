using backend.Domain.Entities;
using backend.Domain.ValueObjects; // NEW: For BoundingBox

namespace backend.Domain.Events;

public class MemberStoryCreatedWithFacesEvent : BaseEvent
{
    public MemberStory MemberStory { get; }
    public IReadOnlyList<FaceDataForCreation> FacesData { get; } // Re-introducing FacesData

    public MemberStoryCreatedWithFacesEvent(MemberStory memberStory, IReadOnlyList<FaceDataForCreation> facesData)
    {
        MemberStory = memberStory;
        FacesData = facesData;
    }

    // Re-introducing nested record to carry face data within the Domain layer
    public record FaceDataForCreation(
        string Id,
        BoundingBox BoundingBox,
        double Confidence,
        string? Thumbnail,
        IReadOnlyList<double> Embedding, // ADDED BACK
        string? Emotion, // ADDED BACK
        double? EmotionConfidence);
}
