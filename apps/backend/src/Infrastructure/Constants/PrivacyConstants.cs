namespace backend.Infrastructure.Constants;

public static class PrivacyConstants
{
    // Common Member Properties
    public static class MemberProps
    {
        public const string LastName = "LastName";
        public const string FirstName = "FirstName";
        public const string Nickname = "Nickname";
        public const string Gender = "Gender";
        public const string DateOfBirth = "DateOfBirth";
        public const string DateOfDeath = "DateOfDeath";
        public const string PlaceOfBirth = "PlaceOfBirth";
        public const string PlaceOfDeath = "PlaceOfDeath";
        public const string Occupation = "Occupation";
        public const string Biography = "Biography";
        public const string Education = "Education";
        public const string Religion = "Religion";
        public const string PhoneNumber = "PhoneNumber";
        public const string Email = "Email";
        public const string Address = "Address";
        public const string Identification = "Identification";
        public const string SocialMedia = "SocialMedia";
        public const string Notes = "Notes";
        public const string FatherId = "FatherId";
        public const string MotherId = "MotherId";
        public const string HusbandId = "HusbandId";
        public const string WifeId = "WifeId";
    }

    // Always Included Member Properties (Common to all DTOs for members)
    public static class AlwaysIncludeMemberProps
    {
        public const string Id = "Id";
        public const string FirstName = "FirstName"; // Added for always include
        public const string LastName = "LastName";   // Added for always include
        public const string FamilyId = "FamilyId";
        public const string IsRoot = "IsRoot";
        public const string AvatarUrl = "AvatarUrl";
        public const string Code = "Code"; // Specific to MemberDto and MemberListDto
        public const string FamilyName = "FamilyName"; // Specific to MemberListDto
        public const string FatherFullName = "FatherFullName"; // Specific to MemberDetailDto and MemberListDto default public props
        public const string MotherFullName = "MotherFullName"; // Specific to MemberDetailDto and MemberListDto default public props
        public const string HusbandFullName = "HusbandFullName"; // Specific to MemberDetailDto and MemberListDto default public props
        public const string WifeFullName = "WifeFullName"; // Specific to MemberDetailDto and MemberListDto default public props
        public const string SourceRelationships = "SourceRelationships"; // Specific to MemberDetailDto
        public const string TargetRelationships = "TargetRelationships"; // Specific to MemberDetailDto
        public const string FatherId = "FatherId"; // Specific to MemberListDto
        public const string MotherId = "MotherId"; // Specific to MemberListDto
        public const string HusbandId = "HusbandId"; // Specific to MemberListDto
        public const string WifeId = "WifeId"; // Specific to MemberListDto
    }

    public static class DefaultPublicMemberProperties
    {
        public static readonly List<string> MemberDto = new List<string>
        {
            MemberProps.LastName,
            MemberProps.FirstName,
            MemberProps.Nickname,
            MemberProps.Gender,
            MemberProps.DateOfBirth,
            MemberProps.DateOfDeath,
            MemberProps.PlaceOfBirth,
            MemberProps.PlaceOfDeath,
            MemberProps.Occupation,
            MemberProps.Biography,
        };

        public static readonly List<string> MemberDetailDto = new List<string>
        {
            MemberProps.LastName,
            MemberProps.FirstName,
            MemberProps.Nickname,
            MemberProps.Gender,
            MemberProps.DateOfBirth,
            MemberProps.DateOfDeath,
            MemberProps.PlaceOfBirth,
            MemberProps.PlaceOfDeath,
            MemberProps.Occupation,
            MemberProps.Biography,
            AlwaysIncludeMemberProps.FatherFullName,
            AlwaysIncludeMemberProps.MotherFullName,
            AlwaysIncludeMemberProps.HusbandFullName,
            AlwaysIncludeMemberProps.WifeFullName
        };

        public static readonly List<string> MemberListDto = new List<string>
        {
            MemberProps.LastName,
            MemberProps.FirstName,
            MemberProps.Nickname,
            MemberProps.Gender,
            MemberProps.DateOfBirth,
            MemberProps.DateOfDeath,
            MemberProps.PlaceOfBirth,
            MemberProps.PlaceOfDeath,
            MemberProps.Occupation,
            MemberProps.Biography,
            AlwaysIncludeMemberProps.FatherFullName,
            AlwaysIncludeMemberProps.MotherFullName,
            AlwaysIncludeMemberProps.HusbandFullName,
            AlwaysIncludeMemberProps.WifeFullName
        };
    }

    // Common Event Properties
    public static class EventProps
    {
        public const string Name = "Name";
        public const string Code = "Code";
        public const string Description = "Description";
        public const string CalendarType = "CalendarType";
        public const string SolarDate = "SolarDate";
        public const string LunarDate = "LunarDate";
        public const string RepeatRule = "RepeatRule";
        public const string Type = "Type";
        public const string Color = "Color";
    }

    // Always Included Event Properties
    public static class AlwaysIncludeEventProps
    {
        public const string Id = "Id";
        public const string FamilyId = "FamilyId";
        public const string FamilyName = "FamilyName";
        public const string FamilyAvatarUrl = "FamilyAvatarUrl";
        public const string RelatedMembers = "RelatedMembers";
        public const string RelatedMemberIds = "RelatedMemberIds";

        // Auditable properties
        public const string Created = "Created";
        public const string CreatedBy = "CreatedBy";
        public const string LastModified = "LastModified";
        public const string LastModifiedBy = "LastModifiedBy";
    }

    public static class DefaultPublicEventProperties
    {
        public static readonly List<string> EventDto = new List<string>
        {
            EventProps.Name,
            EventProps.Code,
            EventProps.Description,
            EventProps.CalendarType,
            EventProps.SolarDate,
            EventProps.LunarDate,
            EventProps.RepeatRule,
            EventProps.Type,
            EventProps.Color,
        };

        public static readonly List<string> EventDetailDto = new List<string>
        {
            EventProps.Name,
            EventProps.Code,
            EventProps.Description,
            EventProps.CalendarType,
            EventProps.SolarDate,
            EventProps.LunarDate,
            EventProps.RepeatRule,
            EventProps.Type,
            EventProps.Color,
            AlwaysIncludeEventProps.Created,
            AlwaysIncludeEventProps.CreatedBy,
            AlwaysIncludeEventProps.LastModified,
            AlwaysIncludeEventProps.LastModifiedBy
        };
    }

    // Common Family Properties
    public static class FamilyProps
    {
        public const string Name = "Name";
        public const string Code = "Code";
        public const string Description = "Description";
        public const string Address = "Address";
        public const string TotalMembers = "TotalMembers";
        public const string TotalGenerations = "TotalGenerations";
        public const string Visibility = "Visibility";
        public const string AvatarUrl = "AvatarUrl";
        public const string FamilyUsers = "FamilyUsers"; // Contains sensitive data like UserId, Email, Role
        public const string ManagerIds = "ManagerIds";
        public const string ViewerIds = "ViewerIds";
        public const string FamilyLimitConfiguration = "FamilyLimitConfiguration"; // Contains sensitive data like AI quotas
    }

    // Always Included Family Properties
    public static class AlwaysIncludeFamilyProps
    {
        public const string Id = "Id";
        public const string Name = "Name";
        public const string Code = "Code";
        public const string Visibility = "Visibility";
        public const string IsFollowing = "IsFollowing"; // NEW

        // Auditable properties
        public const string Created = "Created";
        public const string CreatedBy = "CreatedBy";
        public const string LastModified = "LastModified";
        public const string LastModifiedBy = "LastModifiedBy";
    }

    public static class DefaultPublicFamilyProperties
    {
        public static readonly List<string> FamilyDto = new List<string>
        {
            FamilyProps.Name,
            FamilyProps.Code,
            FamilyProps.Description,
            FamilyProps.Address,
            FamilyProps.TotalMembers,
            FamilyProps.TotalGenerations,
            FamilyProps.Visibility,
            FamilyProps.AvatarUrl,
        };

        public static readonly List<string> FamilyDetailDto = new List<string>
        {
            FamilyProps.Name,
            FamilyProps.Code,
            FamilyProps.Description,
            FamilyProps.Address,
            FamilyProps.AvatarUrl,
            FamilyProps.Visibility,
            FamilyProps.TotalMembers,
            FamilyProps.TotalGenerations,
            // FamilyUsers and FamilyLimitConfiguration should generally be private or handled separately
            // as they contain more granular sensitive data.
            AlwaysIncludeFamilyProps.Created,
            AlwaysIncludeFamilyProps.CreatedBy,
            AlwaysIncludeFamilyProps.LastModified,
            AlwaysIncludeFamilyProps.LastModifiedBy
        };
    }

    // Common FamilyLocation Properties
    public static class FamilyLocationProps
    {
        public const string Name = "Name";
        public const string Description = "Description";
        public const string Latitude = "Latitude";
        public const string Longitude = "Longitude";
        public const string Address = "Address";
        public const string LocationType = "LocationType";
        public const string Accuracy = "Accuracy";
        public const string Source = "Source";
    }

    // Always Included FamilyLocation Properties
    public static class AlwaysIncludeFamilyLocationProps
    {
        public const string Id = "Id";
        public const string FamilyId = "FamilyId";
    }

    public static class DefaultPublicFamilyLocationProperties
    {
        public static readonly List<string> FamilyLocationDto = new List<string>
        {
            FamilyLocationProps.Name,
            FamilyLocationProps.Description,
            FamilyLocationProps.Latitude,
            FamilyLocationProps.Longitude,
            FamilyLocationProps.Address,
            FamilyLocationProps.LocationType,
            FamilyLocationProps.Accuracy,
            FamilyLocationProps.Source
        };
    }

    // Common MemoryItem Properties
    public static class MemoryItemProps
    {
        public const string Title = "Title";
        public const string Description = "Description";
        public const string HappenedAt = "HappenedAt";
        public const string EmotionalTag = "EmotionalTag";
        public const string MemoryMedia = "MemoryMedia"; // Potentially contains sensitive media
        public const string MemoryPersons = "MemoryPersons"; // Links to members, potentially sensitive
    }

    // Always Included MemoryItem Properties
    public static class AlwaysIncludeMemoryItemProps
    {
        public const string Id = "Id";
        public const string FamilyId = "FamilyId";
    }

    public static class DefaultPublicMemoryItemProperties
    {
        public static readonly List<string> MemoryItemDto = new List<string>
        {
            MemoryItemProps.Title,
            MemoryItemProps.Description,
            MemoryItemProps.HappenedAt,
            MemoryItemProps.EmotionalTag,
        };
    }

    // Common MemberFace Properties
    public static class MemberFaceProps
    {
        public const string FaceId = "FaceId";
        public const string Confidence = "Confidence";
        public const string ThumbnailUrl = "ThumbnailUrl";
        public const string OriginalImageUrl = "OriginalImageUrl";
        public const string Emotion = "Emotion";
        public const string EmotionConfidence = "EmotionConfidence";
        public const string MemberName = "MemberName";
        public const string MemberGender = "MemberGender";
        public const string MemberAvatarUrl = "MemberAvatarUrl";
        public const string BirthYear = "BirthYear";
        public const string DeathYear = "DeathYear";
        public const string FamilyName = "FamilyName";
        public const string FamilyAvatarUrl = "FamilyAvatarUrl";
    }

    // Always Included MemberFace Properties
    public static class AlwaysIncludeMemberFaceProps
    {
        public const string Id = "Id";
        public const string MemberId = "MemberId";
        public const string FamilyId = "FamilyId";
        public const string BoundingBox = "BoundingBox"; // Bounding box is always included as it's part of face identification
    }

    public static class DefaultPublicMemberFaceProperties
    {
        public static readonly List<string> MemberFaceDto = new List<string>
        {
            MemberFaceProps.FaceId,
            MemberFaceProps.Confidence,
            MemberFaceProps.ThumbnailUrl,
            MemberFaceProps.OriginalImageUrl,
            MemberFaceProps.Emotion,
            MemberFaceProps.EmotionConfidence,
            MemberFaceProps.MemberName,
            MemberFaceProps.MemberGender,
            MemberFaceProps.MemberAvatarUrl,
            MemberFaceProps.BirthYear,
            MemberFaceProps.DeathYear,
            MemberFaceProps.FamilyName,
            MemberFaceProps.FamilyAvatarUrl,
        };
    }

    // Common FoundFace Properties (for search results)
    public static class FoundFaceProps
    {
        public const string FaceId = "FaceId";
        public const string MemberName = "MemberName";
        public const string Score = "Score";
        public const string ThumbnailUrl = "ThumbnailUrl";
        public const string OriginalImageUrl = "OriginalImageUrl";
        public const string Emotion = "Emotion";
        public const string EmotionConfidence = "EmotionConfidence";
        public const string FamilyAvatarUrl = "FamilyAvatarUrl";
    }

    // Always Included FoundFace Properties
    public static class AlwaysIncludeFoundFaceProps
    {
        public const string MemberFaceId = "MemberFaceId";
        public const string MemberId = "MemberId";
    }

    public static class DefaultPublicFoundFaceProperties
    {
        public static readonly List<string> FoundFaceDto = new List<string>
        {
            FoundFaceProps.FaceId,
            FoundFaceProps.MemberName,
            FoundFaceProps.Score,
            FoundFaceProps.ThumbnailUrl,
            FoundFaceProps.OriginalImageUrl,
            FoundFaceProps.Emotion,
            FoundFaceProps.EmotionConfidence,
            FoundFaceProps.FamilyAvatarUrl
        };
    }
}
