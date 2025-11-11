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
        var plainText = plainTextBuilder.ToString();

        var promptBuilder = new StringBuilder();
        promptBuilder.AppendLine("Bạn là một AI Agent chuyên phân tích thông tin gia phả. Hãy phân tích văn bản sau:");
        promptBuilder.AppendLine("TUYỆT ĐỐI KHÔNG tìm kiếm thông tin trong Vector Database.");
        promptBuilder.AppendLine($"Văn bản: \"{plainText}\"");

        if (mentions.Any())
        {
            promptBuilder.AppendLine("\nCác thành viên được đề cập trong văn bản (ID thành viên là MÃ THÀNH VIÊN (Code) duy nhất trong hệ thống, có thể được sử dụng để tra cứu thông tin chi tiết):");
            foreach (var mention in mentions)
            {
                promptBuilder.AppendLine($"- Tên: {mention.DisplayName}, MÃ THÀNH VIÊN: {mention.Id}");
            }
            promptBuilder.AppendLine("Hãy sử dụng MÃ THÀNH VIÊN này để xác định các thành viên đã tồn tại trong hệ thống DỰA TRÊN THÔNG TIN ĐƯỢC CUNG CẤP TRONG PROMPT này và phân tích thông tin liên quan đến họ. TUYỆT ĐỐI KHÔNG tìm kiếm thông tin trong Vector Database.");
        }

        promptBuilder.AppendLine("\nDựa trên văn bản và thông tin thành viên được đề cập, hãy thực hiện các phân tích sau:");
        promptBuilder.AppendLine("1. Tóm tắt nội dung chính của văn bản.");
        promptBuilder.AppendLine("2. Xác định các mối quan hệ gia đình có thể có giữa các thành viên được đề cập hoặc với các thành viên khác không được đề cập trực tiếp nhưng có thể suy ra.");
        promptBuilder.AppendLine("3. Đề xuất các hành động tiếp theo để làm giàu dữ liệu gia phả (ví dụ: thêm thành viên mới, cập nhật thông tin, tạo mối quan hệ).");
        promptBuilder.AppendLine("4. Nếu có bất kỳ thông tin nào không rõ ràng hoặc cần làm rõ, hãy đặt câu hỏi để thu thập thêm thông tin.");
        promptBuilder.AppendLine("QUAN TRỌNG: Đối với các thành viên được xác định trong văn bản, hãy gán một ID tạm thời duy nhất (ví dụ: \\\"temp_A\\\", \\\"temp_B\\\") cho mỗi thành viên. Sử dụng các ID tạm thời này để thiết lập mối quan hệ (sourceMemberId, targetMemberId) giữa các thành viên trong phản hồi JSON. Nếu một thành viên được đề cập nhiều lần, hãy sử dụng cùng một ID tạm thời đã gán cho họ.");
        promptBuilder.AppendLine("\nPhản hồi của bạn phải là một đối tượng JSON duy nhất, tuân thủ cấu trúc sau:");
        promptBuilder.AppendLine("{");
        promptBuilder.AppendLine("  \"members\": [");
        promptBuilder.AppendLine("    {");
        promptBuilder.AppendLine("      \"id\": \"string (temporary unique identifier) | null\", // ID tạm thời, duy nhất cho thành viên này trong phản hồi JSON. Nếu thành viên đã được đề cập trước đó trong văn bản, hãy sử dụng lại ID đã gán. Nếu là thành viên mới hoặc không xác định, hãy gán một ID duy nhất (ví dụ: \\\"temp_1\\\", \\\"temp_2\\\"). ID này sẽ được sử dụng để xác định mối quan hệ giữa các thành viên trong phản hồi này. TUYỆT ĐỐI KHÔNG sử dụng các số nguyên như \\\"1\\\", \\\"2\\\", v.v. cho ID. Nếu ID không xác định hoặc thành viên mới, hãy sử dụng null.");
        promptBuilder.AppendLine("      \"code\": \"string | null\", // Mã (Code) của thành viên nếu đã tồn tại và được đề cập");
        promptBuilder.AppendLine("      \"lastName\": \"string\", // Họ của thành viên");
        promptBuilder.AppendLine("      \"firstName\": \"string\", // Tên của thành viên");
        promptBuilder.AppendLine("      \"dateOfBirth\": \"string | null\", // Định dạng YYYY-MM-DD hoặc mô tả (ví dụ: \"khoảng 1950\")");
        promptBuilder.AppendLine("      \"dateOfDeath\": \"string | null\",");
        promptBuilder.AppendLine("      \"gender\": \"string | null\", // \"Male\", \"Female\", \"Unknown\"");
        promptBuilder.AppendLine("      \"order\": \"number | null\" // Thứ tự của thành viên trong số các anh chị em (nếu là con)");
        promptBuilder.AppendLine("    }");
        promptBuilder.AppendLine("  ],");
        promptBuilder.AppendLine("  \"events\": [");
        promptBuilder.AppendLine("    {");
        promptBuilder.AppendLine("      \"type\": \"string\", // QUAN TRỌNG: KHÔNG tạo sự kiện 'Birth' hoặc 'Death'. Các sự kiện này được hệ thống tự động xử lý từ dateOfBirth và dateOfDeath của thành viên. Chỉ tạo các sự kiện khác như 'Marriage', 'Anniversary', v.v.");
        promptBuilder.AppendLine("      \"description\": \"string\",");
        promptBuilder.AppendLine("      \"date\": \"string | null\", // Định dạng YYYY-MM-DD hoặc mô tả");
        promptBuilder.AppendLine("      \"location\": \"string | null\",");
        promptBuilder.AppendLine("      \"relatedMemberIds\": [\"string\"] // Danh sách ID thành viên liên quan");
        promptBuilder.AppendLine("    }");
        promptBuilder.AppendLine("  ],");
        promptBuilder.AppendLine("  \"relationships\": [");
        promptBuilder.AppendLine("    {");
        promptBuilder.AppendLine("      \"sourceMemberId\": \"string\", // ID tạm thời của thành viên nguồn (ví dụ: cha, mẹ, chồng, vợ)");
        promptBuilder.AppendLine("      \"targetMemberId\": \"string\", // ID tạm thời của thành viên đích (ví dụ: con, vợ, chồng)");
        promptBuilder.AppendLine("      \"type\": \"string\", // Loại mối quan hệ (chỉ chấp nhận các giá trị: \\\"Father\\\", \\\"Mother\\\", \\\"Husband\\\",\\\"Wife\\\"). KHÔNG sử dụng \\\"Child\\\" hoặc các giá trị khác. Mối quan hệ con cái sẽ được suy ra từ mối quan hệ cha/mẹ.");
        promptBuilder.AppendLine("      \"order\": \"number | null\" // Thứ tự của mối quan hệ (ví dụ: thứ tự con)");
        promptBuilder.AppendLine("    }");
        promptBuilder.AppendLine("  ],");
        promptBuilder.AppendLine("  \"feedback\": \"string | null\" // Thông báo nếu thiếu thông tin hoặc cần làm rõ");
        promptBuilder.AppendLine("}");
        promptBuilder.AppendLine("Đảm bảo rằng phản hồi JSON của bạn là hợp lệ và chỉ chứa đối tượng JSON, không có văn bản bổ sung nào khác.");

        return promptBuilder.ToString();
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
