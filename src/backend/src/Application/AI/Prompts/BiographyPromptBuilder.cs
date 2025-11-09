using backend.Application.AI.Commands;
using backend.Domain.Entities;
using System.Text;
using System.Collections.Generic;

namespace backend.Application.AI.Prompts;

public static class BiographyPromptBuilder
{
    public static string BuildPrompt(GenerateBiographyCommand request, Member member, Family? family, Member? father, Member? mother, List<Member> spouses)
    {
        var promptBuilder = new StringBuilder();
        promptBuilder.AppendLine("Generate a biography for the following family member.");
        promptBuilder.AppendLine($"Style: {request.Style}");
        promptBuilder.AppendLine("Output language: Vietnamese");
        promptBuilder.AppendLine("Please limit the biography to approximately 500 words.");

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
