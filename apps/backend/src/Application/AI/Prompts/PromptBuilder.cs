using backend.Application.NaturalLanguage.Commands.AnalyzeNaturalLanguage;
using System.Text;
using backend.Application.AI.Commands; // Add using directive for GenerateBiographyCommand
using backend.Domain.Entities; // Add using directive for Member and Family
using System.Text.RegularExpressions; // Add using directive for Regex

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
}
