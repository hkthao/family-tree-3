using System.Text;
using System.Text.Json; // NEW
using System.Text.RegularExpressions; // Add using directive for Regex
using backend.Application.AI.Commands; // Add using directive for GenerateBiographyCommand
using backend.Application.AI.DTOs; // NEW USING for AiPhotoAnalysisInputDto
using backend.Application.AI.Models; // NEW USING
using backend.Domain.Entities; // Add using directive for Member and Family

namespace backend.Application.AI.Prompts;

/// <summary>
/// Lớp tiện ích để xây dựng các prompt cho AI Agent.
/// </summary>
public static class PromptBuilder
{
    /// <summary>
    /// Xây dựng prompt cho AI Agent dựa trên văn bản và các mention đã trích xuất từ phân tích ngôn ngữ tự nhiên.
    /// </summary>
    /// <param name="content">Văn bản thô chứa các mention.</param>
    /// <returns>Chuỗi prompt cho AI Agent.</returns>
    public static string BuildNaturalLanguageAnalysisPrompt(string content)
    {
        var mentions = new List<MentionData>();
        var plainTextBuilder = new StringBuilder();
        var lastIndex = 0;

        // Regex để tìm các mention theo định dạng @[DisplayName](ID)
        var regex = new Regex(@"@\[([^\]]+)\]\(([^)]+)\)");

        foreach (Match match in regex.Matches(content))
        {
            // Thêm phần văn bản trước mention vào plainText
            plainTextBuilder.Append(content.Substring(lastIndex, match.Index - lastIndex));

            var displayName = match.Groups[1].Value;
            var id = match.Groups[2].Value;

            mentions.Add(new MentionData(id, displayName));

            // Thay thế mention bằng displayName trong plainText
            plainTextBuilder.Append($"@{displayName}");

            lastIndex = match.Index + match.Length;
        }

        // Thêm phần văn bản còn lại sau mention cuối cùng
        plainTextBuilder.Append(content.Substring(lastIndex));
        return plainTextBuilder.ToString();
    }

    /// <summary>
    /// Xây dựng prompt cho AI Agent để tạo tiểu sử cho một thành viên gia đình.
    /// </summary>
    public static string BuildBiographyPrompt(GenerateBiographyCommand request, Member member, Family? family, Member? father, Member? mother, List<Member> spouses)
    {
        var promptBuilder = new StringBuilder();
        promptBuilder.AppendLine("Generate a biography for the following family member.");
        promptBuilder.AppendLine($"Style: {request.Style}");
        promptBuilder.AppendLine("Output language: Vietnamese");
        promptBuilder.AppendLine("Please limit the biography to approximately 500 words.");
        promptBuilder.AppendLine("Do not search for member information in Qdrant Vector Store.");

        if (!string.IsNullOrEmpty(request.UserPrompt))
        {
            promptBuilder.AppendLine($"Additional instructions: {request.UserPrompt}");
        }

        promptBuilder.AppendLine("\nMember Details:");
        promptBuilder.AppendLine($"- Full Name: {member.FullName}");
        promptBuilder.AppendLine($"- Date of Birth: {member.DateOfBirth?.ToShortDateString() ?? "Unknown"}");
        promptBuilder.AppendLine($"- Date of Death: {member.DateOfDeath?.ToShortDateString() ?? "N/A"}");
        promptBuilder.AppendLine($"- Gender: {member.Gender ?? "Unknown"}");
        promptBuilder.AppendLine($"- Place of Birth: {member.PlaceOfBirth ?? "Unknown"}");
        promptBuilder.AppendLine($"- Place of Death: {member.PlaceOfDeath ?? "N/A"}");
        promptBuilder.AppendLine($"- Occupation: {member.Occupation ?? "Unknown"}");

        if (family != null)
        {
            promptBuilder.AppendLine($"- Family: {family.Name}");
            promptBuilder.AppendLine($"- Family Description: {family.Description ?? "N/A"}");
        }

        if (father != null)
        {
            promptBuilder.AppendLine($"- Father: {father.FullName}");
        }
        if (mother != null)
        {
            promptBuilder.AppendLine($"- Mother: {mother.FullName}");
        }

        if (spouses.Any())
        {
            promptBuilder.AppendLine("- Spouses:");
            foreach (var spouse in spouses)
            {
                promptBuilder.AppendLine($"  - {spouse.FullName}");
            }
        }

        if (request.GeneratedFromDB && !string.IsNullOrEmpty(member.Biography))
        {
            promptBuilder.AppendLine($"- Existing Biography: {member.Biography}");
            promptBuilder.AppendLine("Please use this existing biography as a base and enhance it, or rewrite it based on the provided style and additional instructions.");
        }
        else if (request.GeneratedFromDB)
        {
            promptBuilder.AppendLine("No existing biography found in the database. Generate a new one based on the details.");
        }
        else
        {
            promptBuilder.AppendLine("Do not use any existing biography from the database. Generate a new one based on the details and user prompt.");
        }

        return promptBuilder.ToString();
    }

    /// <summary>
    /// Xây dựng prompt cho AI Agent để phân tích ảnh dựa trên dữ liệu AiPhotoAnalysisInputDto.
    /// </summary>
    /// <param name="input">Đối tượng AiPhotoAnalysisInputDto chứa dữ liệu ảnh và các thông tin liên quan.</param>
    /// <returns>Chuỗi prompt (user message) cho AI Agent.</returns>
    public static string BuildPhotoAnalysisPrompt(AiPhotoAnalysisInputDto input)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase, // Ensure camelCase for JSON keys
            WriteIndented = false // Keep JSON concise
        };
        var jsonInput = System.Text.Json.JsonSerializer.Serialize(input, options);

        var promptBuilder = new StringBuilder();
        promptBuilder.AppendLine("Hãy phân tích bức ảnh dựa trên dữ liệu JSON sau:");
        promptBuilder.AppendLine(jsonInput);
        promptBuilder.AppendLine("Trả về kết quả phân tích theo đúng định dạng JSON đã được hướng dẫn trong system prompt.");
        return promptBuilder.ToString();
    }

    /// <summary>
    /// Xây dựng prompt cho AI Agent để tạo câu chuyện về một thành viên gia đình.
    /// </summary>
    /// <param name="request">Lệnh tạo câu chuyện chứa thông tin yêu cầu.</param>
    /// <param name="member">Thông tin chi tiết về thành viên.</param>
    /// <param name="family">Thông tin chi tiết về gia đình.</param>
    /// <param name="photoAnalysisResult">Kết quả phân tích ảnh liên quan (nếu có).</param>
    /// <returns>Chuỗi prompt (user message) cho AI Agent.</returns>
    public static string BuildStoryGenerationPrompt(
        backend.Application.Memories.Commands.GenerateStory.GenerateStoryCommand request,
        Member member,
        Family? family)
    {
        var promptBuilder = new StringBuilder();
        promptBuilder.AppendLine($"Tạo một câu chuyện về thành viên gia đình sau với phong cách: {request.Style}");
        promptBuilder.AppendLine($"Độ dài câu chuyện khoảng {request.MaxWords} từ.");
        promptBuilder.AppendLine("Ngôn ngữ đầu ra: Tiếng Việt.");
        promptBuilder.AppendLine("\nThông tin thành viên:");
        promptBuilder.AppendLine($"- Tên đầy đủ: {member.FullName}");
        promptBuilder.AppendLine($"- Ngày sinh: {member.DateOfBirth?.ToShortDateString() ?? "Không rõ"}");
        promptBuilder.AppendLine($"- Ngày mất: {member.DateOfDeath?.ToShortDateString() ?? "N/A"}");
        promptBuilder.AppendLine($"- Giới tính: {member.Gender ?? "Không rõ"}");
        promptBuilder.AppendLine($"- Nơi sinh: {member.PlaceOfBirth ?? "Không rõ"}");
        promptBuilder.AppendLine($"- Nghề nghiệp: {member.Occupation ?? "Không rõ"}");

        if (family != null)
        {
            promptBuilder.AppendLine($"- Gia đình: {family.Name}");
        }

        if (!string.IsNullOrEmpty(member.HusbandFullName))
        {
            promptBuilder.AppendLine($"- Vợ/chồng (Chồng): {member.HusbandFullName}");
        }
        if (!string.IsNullOrEmpty(member.WifeFullName))
        {
            promptBuilder.AppendLine($"- Vợ/chồng (Vợ): {member.WifeFullName}");
        }

        if (!string.IsNullOrEmpty(request.Perspective))
        {
            promptBuilder.AppendLine($"\nGóc nhìn bài viết: {request.Perspective}");
        }



        if (!string.IsNullOrEmpty(request.RawText))
        {
            promptBuilder.AppendLine("\nThông tin bổ sung từ người dùng:");
            promptBuilder.AppendLine(request.RawText);
        }

        // --- Photo Analysis Result Details ---
        if (request.ResizedImageUrl != null || (request.PhotoPersons != null && request.PhotoPersons.Any()))
        {
            promptBuilder.AppendLine("\nKết quả phân tích ảnh liên quan:");
            if (!string.IsNullOrEmpty(request.ResizedImageUrl))
            {
                promptBuilder.AppendLine($"- URL ảnh đã điều chỉnh kích thước: {request.ResizedImageUrl}");
            }
            if (request.PhotoPersons != null && request.PhotoPersons.Any())
            {
                promptBuilder.AppendLine("- Người trong ảnh:");
                foreach (var person in request.PhotoPersons)
                {
                    promptBuilder.AppendLine($"  - {person.Name} (Cảm xúc: {person.Emotion ?? "Không rõ"}, Độ tự tin: {person.Confidence ?? 0:P}{(string.IsNullOrEmpty(person.RelationPrompt) ? "" : $", Quan hệ: {person.RelationPrompt}")})");
                }
            }
        }

        // Add instructions from the system prompt
        promptBuilder.AppendLine("\nTuân thủ các quy tắc đã được đặt ra trong system prompt để tạo ra câu chuyện.");

        return promptBuilder.ToString();
    }
}
