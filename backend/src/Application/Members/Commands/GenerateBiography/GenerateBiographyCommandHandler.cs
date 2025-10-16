using backend.Application.AI.Common;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Common.Services; // Added
using backend.Domain.Enums;
using backend.Domain.Extensions; // Added // Added
using System.Text;

namespace backend.Application.Members.Commands.GenerateBiography;

public class GenerateBiographyCommandHandler : IRequestHandler<GenerateBiographyCommand, Result<BiographyResultDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly IAuthorizationService _authorizationService;
    private readonly IChatProviderFactory _chatProviderFactory;
    private readonly FamilyAuthorizationService _familyAuthorizationService; // Added

    public GenerateBiographyCommandHandler(
        IApplicationDbContext context,
        IUser user,
        IAuthorizationService authorizationService,
        IChatProviderFactory chatProviderFactory,
        FamilyAuthorizationService familyAuthorizationService) // Added
    {
        _context = context;
        _user = user;
        _authorizationService = authorizationService;
        _chatProviderFactory = chatProviderFactory;
        _familyAuthorizationService = familyAuthorizationService; // Added
    }

    public async Task<Result<BiographyResultDto>> Handle(GenerateBiographyCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _user.Id;
        if (string.IsNullOrEmpty(currentUserId))
        {
            return Result<BiographyResultDto>.Failure("User is not authenticated.", "Authentication");
        }

        var currentUserProfile = await _authorizationService.GetCurrentUserProfileAsync(cancellationToken);
        if (currentUserProfile == null)
        {
            return Result<BiographyResultDto>.Failure("User profile not found.", "NotFound");
        }

        var member = await _context.Members.FindAsync(new object[] { request.MemberId }, cancellationToken);
        if (member == null)
        {
            return Result<BiographyResultDto>.Failure($"Member with ID {request.MemberId} not found.", "NotFound");
        }

        // Authorization check: User must be a manager of the family the member belongs to, or an admin.
        var authorizationResult = await _familyAuthorizationService.AuthorizeFamilyAccess(member.FamilyId, cancellationToken);
        if (!authorizationResult.IsSuccess)
        {
            return Result<BiographyResultDto>.Failure(authorizationResult.Error ?? "Unknown authorization error.", authorizationResult.ErrorSource ?? "Authorization");
        }

        // --- AI Biography Generation Logic ---
        var chatProvider = _chatProviderFactory.GetProvider(ChatAIProvider.Local); // Using Local provider for now

        var systemPromptBuilder = new StringBuilder();
        systemPromptBuilder.AppendLine("You are an AI assistant specialized in writing biographies for family tree members.");
        systemPromptBuilder.AppendLine("Your goal is to create a compelling and informative biography, up to 500 words, based on the provided information and desired tone.");
        systemPromptBuilder.AppendLine("Focus on key life events, relationships, and personal characteristics.");
        systemPromptBuilder.AppendLine("If specific details are missing, you can use general knowledge or infer plausible information, but clearly state any assumptions.");
        systemPromptBuilder.AppendLine("Always respond with ONLY the biography text. Do not include any conversational text or greetings.");
        systemPromptBuilder.AppendLine("Always respond in Vietnamese."); // Added

        switch (request.Tone)
        {
            case BiographyTone.Emotional:
                systemPromptBuilder.AppendLine("Write the biography with an emotional and heartfelt tone, highlighting personal feelings, significant bonds, and the impact of events on the individual and their loved ones.");
                break;
            case BiographyTone.Historical:
                systemPromptBuilder.AppendLine("Write the biography with a historical and factual tone, focusing on chronological events, societal context, and the individual's contributions or experiences within their time.");
                break;
            case BiographyTone.Storytelling:
                systemPromptBuilder.AppendLine("Write the biography in a narrative, storytelling style, engaging the reader with a compelling personal journey, anecdotes, and vivid descriptions.");
                break;
            case BiographyTone.Formal: // Added
                systemPromptBuilder.AppendLine("Write the biography in a formal and respectful tone, suitable for official records or academic contexts, using precise language and avoiding colloquialisms.");
                break;
            case BiographyTone.Informal: // Added
                systemPromptBuilder.AppendLine("Write the biography in an informal and engaging tone, as if telling a personal story to a close acquaintance, using conversational language and a friendly approach.");
                break;
            case BiographyTone.Neutral:
            default:
                systemPromptBuilder.AppendLine("Write the biography in a neutral, objective, and informative tone, presenting facts clearly and concisely.");
                break;
        }

        var systemPrompt = systemPromptBuilder.ToString();
        var userPromptBuilder = new StringBuilder();

        userPromptBuilder.AppendLine($"Write a biography for the following member: {member.FullName}.");

        if (request.UseSystemData)
        {
            userPromptBuilder.AppendLine("Here is additional system data about the member to incorporate:");
            userPromptBuilder.AppendLine($"- Full Name: {member.FullName}");
            userPromptBuilder.AppendLine($"- Gender: {member.Gender}");
            userPromptBuilder.AppendLine($"- Date of Birth: {member.DateOfBirth?.ToShortDateString() ?? "Unknown"}");
            userPromptBuilder.AppendLine($"- Place of Birth: {member.PlaceOfBirth ?? "Unknown"}");
            userPromptBuilder.AppendLine($"- Date of Death: {member.DateOfDeath?.ToShortDateString() ?? "N/A"}");
            userPromptBuilder.AppendLine($"- Place of Death: {member.PlaceOfDeath ?? "N/A"}");
            userPromptBuilder.AppendLine($"- Occupation: {member.Occupation ?? "Unknown"}");

            // Fetch relationships
            var relationships = await _context.Relationships
                .Where(r => r.SourceMemberId == member.Id || r.TargetMemberId == member.Id)
                .Include(r => r.SourceMember)
                .Include(r => r.TargetMember)
                .ToListAsync(cancellationToken);

            if (relationships.Any())
            {
                userPromptBuilder.AppendLine("Relationships:");
                foreach (var rel in relationships)
                {
                    var relatedMemberName = rel.SourceMemberId == member.Id ? rel.TargetMember?.FullName : rel.SourceMember?.FullName;
                    var relationshipType = rel.SourceMemberId == member.Id ? rel.Type.ToString() : rel.Type.GetInverseRelationshipType().ToString();
                    userPromptBuilder.AppendLine($"- {member.FullName} is {relationshipType} of {relatedMemberName} (from {rel.StartDate?.ToShortDateString() ?? "Unknown"} to {rel.EndDate?.ToShortDateString() ?? "Present"}).");
                }
            }
            // Add other relevant data if available (e.g., events, achievements)
        }

        if (!string.IsNullOrWhiteSpace(request.Prompt))
        {
            userPromptBuilder.AppendLine($"Additionally, consider the following specific instructions: {request.Prompt}");
        }

        var userPrompt = userPromptBuilder.ToString();

        var chatMessages = new List<ChatMessage>
        {
            new() { Role = "system", Content = systemPrompt },
            new() { Role = "user", Content = userPrompt }
        };

        string biographyText = await chatProvider.GenerateResponseAsync(chatMessages);
        biographyText = biographyText.Trim();

        if (string.IsNullOrWhiteSpace(biographyText))
        {
            return Result<BiographyResultDto>.Failure("AI did not return a biography.", "NoContent");
        }

        // Ensure biography is within 500 words (approximate)
        var words = biographyText.Split(new char[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        if (words.Length > 500)
        {
            biographyText = string.Join(" ", words.Take(500)) + "...";
        }

        return Result<BiographyResultDto>.Success(new BiographyResultDto { Content = biographyText });
    }
}
