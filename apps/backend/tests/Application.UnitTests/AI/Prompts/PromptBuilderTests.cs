using System.Text.Json;
using backend.Application.AI.Commands;
using backend.Application.AI.DTOs;
using backend.Application.AI.Prompts;
using backend.Application.MemberStories.Commands.GenerateStory;
using backend.Domain.Entities;
using backend.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace backend.Application.UnitTests.AI.Prompts;

public class PromptBuilderTests
{
    [Fact]
    public void BuildNaturalLanguageAnalysisPrompt_ShouldExtractMentionsAndReturnPlainText()
    {
        // Arrange
        var contentWithMentions = "Hello @[John Doe](member_123) and @[Jane Smith](member_456)! This is a test.";
        var expectedPlainText = "Hello @John Doe and @Jane Smith! This is a test.";

        // Act
        var result = PromptBuilder.BuildNaturalLanguageAnalysisPrompt(contentWithMentions);

        // Assert
        result.Should().Be(expectedPlainText);
    }

    [Fact]
    public void BuildNaturalLanguageAnalysisPrompt_ShouldReturnOriginalContent_WhenNoMentions()
    {
        // Arrange
        var contentWithoutMentions = "Hello everyone! This is a test.";

        // Act
        var result = PromptBuilder.BuildNaturalLanguageAnalysisPrompt(contentWithoutMentions);

        // Assert
        result.Should().Be(contentWithoutMentions);
    }

    [Fact]
    public void BuildBiographyPrompt_ShouldBuildCorrectPrompt_WithAllDetails()
    {
        // Arrange
        var memberId = Guid.NewGuid();
        var familyId = Guid.NewGuid();
        var fatherId = Guid.NewGuid();
        var motherId = Guid.NewGuid();
        var spouse1Id = Guid.NewGuid();
        var spouse2Id = Guid.NewGuid();

        var family = new Family { Id = familyId, Name = "Test Family", Description = "A family of testers" };
        var father = new Member("Father", "Test", "FT", familyId, null, "Male", null, null, null, null, null, null, null, null, null, null, null, true) { Id = fatherId };
        var mother = new Member("Mother", "Test", "MT", familyId, null, "Female", null, null, null, null, null, null, null, null, null, null, null, true) { Id = motherId };
        var spouse1 = new Member("Spouse1", "Test", "S1T", familyId, null, "Female", null, null, null, null, null, null, null, null, null, null, null, true) { Id = spouse1Id };
        var spouse2 = new Member("Spouse2", "Test", "S2T", familyId, null, "Male", null, null, null, null, null, null, null, null, null, null, null, true) { Id = spouse2Id };

        var member = new Member(memberId, "Child", "Test", "CT", familyId, family, false);
        member.Update("Child", "Test", "CT", null, "Male", new DateTime(1990, 1, 1), new DateTime(2050, 12, 31), "Test City", "Test Hospital", null, null, null, "Tester", null, "Existing draft biography.", null, false);

        var command = new GenerateBiographyCommand
        {
            MemberId = memberId,
            Style = BiographyStyle.Formal,
            UserPrompt = "Highlight achievements.",
            GeneratedFromDB = true
        };

        var spousesList = new List<Member> { spouse1, spouse2 };

        // Act
        var prompt = PromptBuilder.BuildBiographyPrompt(command, member, family, father, mother, spousesList);

        // Assert
        prompt.Should().Contain("Generate a biography for the following family member.");
        prompt.Should().Contain("Style: Formal");
        prompt.Should().Contain("Output language: Vietnamese");
        prompt.Should().Contain("Additional instructions: Highlight achievements.");
        prompt.Should().Contain($"- Full Name: {member.FullName}");
        prompt.Should().Contain($"- Date of Birth: 1/1/1990");
        prompt.Should().Contain($"- Date of Death: 12/31/2050");
        prompt.Should().Contain($"- Gender: Male");
        prompt.Should().Contain($"- Place of Birth: Test City");
        prompt.Should().Contain($"- Place of Death: Test Hospital");
        prompt.Should().Contain($"- Occupation: Tester");
        prompt.Should().Contain($"- Family: Test Family");
        prompt.Should().Contain($"- Family Description: A family of testers");
        prompt.Should().Contain($"- Father: Father Test");
        prompt.Should().Contain($"- Mother: Mother Test");
        prompt.Should().Contain("- Spouses:");
        prompt.Should().Contain("  - Spouse1 Test");
        prompt.Should().Contain("  - Spouse2 Test");
        prompt.Should().Contain("- Existing Biography: Existing draft biography.");
        prompt.Should().Contain("Please use this existing biography as a base and enhance it, or rewrite it based on the provided style and additional instructions.");
    }

    [Fact]
    public void BuildBiographyPrompt_ShouldHandleMissingDetailsGracefully()
    {
        // Arrange
        var memberId = Guid.NewGuid();
        var member = new Member(memberId, "Orphan", "Child", "OC", Guid.NewGuid(), new Family { Id = Guid.NewGuid() }, false);
        member.Update("Orphan", "Child", "OC", null, null, null, null, null, null, null, null, null, null, null, null, null, false);
        var command = new GenerateBiographyCommand
        {
            MemberId = memberId,
            Style = BiographyStyle.Emotional,
            UserPrompt = "",
            GeneratedFromDB = false
        };

        // Act
        var prompt = PromptBuilder.BuildBiographyPrompt(command, member, null, null, null, new List<Member>());

        // Assert
        prompt.Should().Contain("- Date of Birth: Unknown");
        prompt.Should().Contain("- Date of Death: N/A");
        prompt.Should().Contain("- Gender: Unknown");
        prompt.Should().Contain("- Place of Birth: Unknown");
        prompt.Should().Contain("- Place of Death: N/A");
        prompt.Should().Contain("- Occupation: Unknown");
        prompt.Should().NotContain("- Family:");
        prompt.Should().NotContain("- Father:");
        prompt.Should().NotContain("- Mother:");
        prompt.Should().NotContain("- Spouses:");
        prompt.Should().Contain("Do not use any existing biography from the database. Generate a new one based on the details and user prompt.");
    }

    [Fact]
    public void BuildPhotoAnalysisPrompt_ShouldSerializeInputCorrectly()
    {
        // Arrange
        var input = new AiPhotoAnalysisInputDto
        {
            ImageUrl = "http://test.com/img.jpg",
            ImageSize = "1024x768",
            Faces = new List<AiDetectedFaceDto>
            {
                new AiDetectedFaceDto { FaceId = "f1", Bbox = new List<int> { 10, 20, 30, 40 }, EmotionLocal = new AiEmotionLocalDto { Dominant = "happy", Confidence = 0.95 } }
            },
            MemberInfo = new AiMemberInfoDto { Id = "m1", Name = "Test Member" },
            Exif = new AiExifInfoDto { Datetime = "2023:01:01 12:00:00" }
        };

        // Act
        var prompt = PromptBuilder.BuildPhotoAnalysisPrompt(input);

        // Assert
        prompt.Should().Contain("Hãy phân tích bức ảnh dựa trên dữ liệu JSON sau:");
        prompt.Should().Contain("Trả về kết quả phân tích theo đúng định dạng JSON đã được hướng dẫn trong system prompt.");

        // Deserialize the JSON part of the prompt to verify content
        var jsonPart = prompt.Split("JSON sau:\n")[1].Split("\nTrả về kết quả phân tích")[0].Trim();
        var deserializedInput = JsonSerializer.Deserialize<AiPhotoAnalysisInputDto>(jsonPart, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        deserializedInput.Should().NotBeNull();
        deserializedInput!.ImageUrl.Should().Be(input.ImageUrl);
        deserializedInput.Faces.Should().HaveCount(1);
        deserializedInput.Faces[0].FaceId.Should().Be(input.Faces[0].FaceId);
        deserializedInput.MemberInfo!.Name.Should().Be(input.MemberInfo!.Name);
        deserializedInput.Exif!.Datetime.Should().Be(input.Exif!.Datetime);
    }

    [Fact]
    public void BuildStoryGenerationPrompt_ShouldBuildCorrectPrompt_WithAllDetails()
    {
        // Arrange
        var memberId = Guid.NewGuid();
        var familyId = Guid.NewGuid();

        var family = new Family { Id = familyId, Name = "Gia đình Nguyễn" };
        var member = new Member(memberId, "Thị Nở", "Nguyễn", "TN", familyId, family, false);
        member.Update("Thị Nở", "Nguyễn", "TN", null, "Female", new DateTime(1980, 5, 10), null, "Làng Vũ Đại", null, null, null, null, "Nội trợ", null, null, null, false);
        member.HusbandFullName = "Chí Phèo";

        var photoPersons = new List<PhotoAnalysisPersonDto>
        {
            new PhotoAnalysisPersonDto { Id = "p1", Name = "Thị Nở", Emotion = "Vui vẻ", Confidence = 0.9, RelationPrompt = "nhân vật chính" },
            new PhotoAnalysisPersonDto { Id = "p2", Name = "Chí Phèo", Emotion = "Đăm chiêu", Confidence = 0.7, RelationPrompt = "chồng của Thị Nở" }
        };

        var command = new GenerateStoryCommand
        {
            MemberId = memberId,
            RawText = "Một ngày đẹp trời tại Làng Vũ Đại.",
            Style = "nostalgic",
            Perspective = "first_person",
            MemberName = "Thị Nở",
            ResizedImageUrl = "http://example.com/resized.jpg",
            PhotoPersons = photoPersons,
            MaxWords = 600
        };

        // Act
        var prompt = PromptBuilder.BuildStoryGenerationPrompt(command, member, family);

        // Assert
        prompt.Should().Contain("Tạo một câu chuyện về thành viên gia đình sau với phong cách: nostalgic");
        prompt.Should().Contain("Độ dài câu chuyện khoảng 600 từ.");
        prompt.Should().Contain("Ngôn ngữ đầu ra: Tiếng Việt.");
        prompt.Should().Contain($"- Tên đầy đủ: {member.FullName}");
        prompt.Should().Contain($"- Ngày sinh: 5/10/1980");
        prompt.Should().Contain($"- Giới tính: Female");
        prompt.Should().Contain($"- Nơi sinh: Làng Vũ Đại");
        prompt.Should().Contain($"- Nghề nghiệp: Nội trợ");
        prompt.Should().Contain($"- Gia đình: Gia đình Nguyễn");
        prompt.Should().Contain($"- Vợ/chồng (Chồng): Chí Phèo");
        prompt.Should().Contain("Góc nhìn bài viết: first_person");
        prompt.Should().Contain("Thông tin bổ sung từ người dùng:\nMột ngày đẹp trời tại Làng Vũ Đại.");
        prompt.Should().Contain("Kết quả phân tích ảnh liên quan:");
        prompt.Should().Contain("- URL ảnh đã điều chỉnh kích thước: http://example.com/resized.jpg");
        prompt.Should().Contain("- Người trong ảnh:");
        prompt.Should().Contain("  - Thị Nở (Cảm xúc: Vui vẻ, Độ tự tin: 90.000%, Quan hệ: nhân vật chính)");
        prompt.Should().Contain("  - Chí Phèo (Cảm xúc: Đăm chiêu, Độ tự tin: 70.000%, Quan hệ: chồng của Thị Nở)");
        prompt.Should().Contain("Tuân thủ các quy tắc đã được đặt ra trong system prompt để tạo ra câu chuyện.");
    }

    [Fact]
    public void BuildStoryGenerationPrompt_ShouldHandleMissingPhotoAnalysisDataGracefully()
    {
        // Arrange
        var memberId = Guid.NewGuid();
        var familyId = Guid.NewGuid();

        var family = new Family { Id = familyId, Name = "Gia đình Nguyễn" };
        var member = new Member(memberId, "Thị Nở", "Nguyễn", "TN", familyId, family, false);
        member.Update("Thị Nở", "Nguyễn", "TN", null, "Female", new DateTime(1980, 5, 10), null, "Làng Vũ Đại", null, null, null, null, "Nội trợ", null, null, null, false);

        var command = new GenerateStoryCommand
        {
            MemberId = memberId,
            RawText = "Một ngày bình thường.",
            Style = "formal",
            Perspective = null,
            MemberName = "Thị Nở",
            ResizedImageUrl = null,
            PhotoPersons = null,
            MaxWords = 300
        };

        // Act
        var prompt = PromptBuilder.BuildStoryGenerationPrompt(command, member, family);

        // Assert
        prompt.Should().NotContain("Góc nhìn bài viết:");
        prompt.Should().NotContain("Kết quả phân tích ảnh liên quan:");
        prompt.Should().Contain("Thông tin bổ sung từ người dùng:\nMột ngày bình thường.");
    }
}
