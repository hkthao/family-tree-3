using System.Globalization;
using System.Text;
using System.Text.RegularExpressions; // Add using directive for Regex
using backend.Application.AI.Commands; // Add using directive for GenerateBiographyCommand
using backend.Domain.Entities; // Add using directive for Member and Family

namespace backend.Application.AI.Prompts;

/// <summary>
/// Lớp tiện ích để xây dựng các prompt cho AI Agent.
/// </summary>
public static class PromptBuilder
{
    private record MentionData(string Id, string DisplayName);

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

        promptBuilder.AppendLine("\nChi tiết thành viên:");
        promptBuilder.AppendLine($"- Tên đầy đủ: {member.FullName}");
        promptBuilder.AppendLine($"- Ngày sinh: {member.DateOfBirth?.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture) ?? "Không rõ"}");
        promptBuilder.AppendLine($"- Ngày mất: {member.DateOfDeath?.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture) ?? "N/A"}");
        promptBuilder.AppendLine($"- Giới tính: {member.Gender ?? "Không rõ"}");
        promptBuilder.AppendLine($"- Nơi sinh: {member.PlaceOfBirth ?? "Không rõ"}");
        promptBuilder.AppendLine($"- Nơi mất: {member.PlaceOfDeath ?? "N/A"}");
        promptBuilder.AppendLine($"- Nghề nghiệp: {member.Occupation ?? "Không rõ"}");

        if (family != null)
        {
            promptBuilder.AppendLine($"- Gia đình: {family.Name}");
            promptBuilder.AppendLine($"- Mô tả gia đình: {family.Description ?? "N/A"}");
        }

        if (father != null)
        {
            promptBuilder.AppendLine($"- Cha: {father.FullName}");
        }
        if (mother != null)
        {
            promptBuilder.AppendLine($"- Mẹ: {mother.FullName}");
        }

        if (spouses.Any())
        {
            promptBuilder.AppendLine("- Vợ/chồng:");
            foreach (var spouse in spouses)
            {
                promptBuilder.AppendLine($"  - {spouse.FullName}");
            }
        }

        if (request.GeneratedFromDB && !string.IsNullOrEmpty(member.Biography))
        {
            promptBuilder.AppendLine($"- Tiểu sử hiện có: {member.Biography}");
            promptBuilder.AppendLine("Vui lòng sử dụng tiểu sử hiện có này làm cơ sở và nâng cao nó, hoặc viết lại dựa trên phong cách và các hướng dẫn bổ sung được cung cấp.");
        }
        else if (request.GeneratedFromDB)
        {
            promptBuilder.AppendLine("Không tìm thấy tiểu sử hiện có trong cơ sở dữ liệu. Vui lòng tạo một tiểu sử mới dựa trên các chi tiết.");
        }
        else
        {
            promptBuilder.AppendLine("Không sử dụng bất kỳ tiểu sử hiện có nào từ cơ sở dữ liệu. Tạo một tiểu sử mới dựa trên các chi tiết và lời nhắc của người dùng.");
        }

        return promptBuilder.ToString();
    }
}
