using backend.Application.AI.Common;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Domain.Entities;
using backend.Domain.Events;

namespace backend.Application.Members.Commands.GenerateBiography
{
    public class GenerateBiographyCommandHandler : IRequestHandler<GenerateBiographyCommand, Result<BiographyResultDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IAIContentGenerator _aiContentGenerator;
        private readonly IUser _user;
        private readonly IAuthorizationService _authorizationService;

        public GenerateBiographyCommandHandler(
            IApplicationDbContext context,
            IAIContentGenerator aiContentGenerator,
            IUser user,
            IAuthorizationService authorizationService)
        {
            _context = context;
            _aiContentGenerator = aiContentGenerator;
            _user = user;
            _authorizationService = authorizationService;
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

            var member = await _context.Members.FindAsync(request.MemberId, cancellationToken);
            if (member == null)
            {
                return Result<BiographyResultDto>.Failure($"Member with ID {request.MemberId} not found.", "NotFound");
            }

            // Authorization check: User must be a manager of the family the member belongs to, or an admin.
            if (!_authorizationService.IsAdmin() && !_authorizationService.CanManageFamily(member.FamilyId, currentUserProfile))
            {
                return Result<BiographyResultDto>.Failure("Access denied. Only family managers or admins can generate biographies.", "Forbidden");
            }

            string finalPrompt;
            bool generatedFromDB = false;

            if (!string.IsNullOrEmpty(request.UserPrompt))
            {
                finalPrompt = request.UserPrompt;
            }
            else
            {
                // Auto-generate prompt from DB data
                finalPrompt = await GeneratePromptFromDBData(member, cancellationToken);
                generatedFromDB = true;
            }

            // Construct AI request
            var aiRequest = new AIRequest
            {
                UserPrompt = finalPrompt,
                Style = request.Style,
                Language = request.Language,
                GeneratedFromDB = generatedFromDB,
                MemberId = request.MemberId
            };

            // Generate content
            var aiResult = await _aiContentGenerator.GenerateContentAsync(aiRequest, cancellationToken);

            if (!aiResult.IsSuccess)
            {
                return Result<BiographyResultDto>.Failure(aiResult.Error ?? "Unknown AI generation error", aiResult.ErrorSource ?? "Unknown");
            }

            // Save biography to DB
            var biography = new AIBiography
            {
                MemberId = request.MemberId,
                Style = request.Style,
                Content = aiResult.Value!.Content,
                Provider = aiResult.Value!.Provider,
                UserPrompt = request.UserPrompt ?? finalPrompt, // Store original user prompt or generated prompt
                GeneratedFromDB = generatedFromDB,
                TokensUsed = aiResult.Value!.TokensUsed,
                Metadata = null // TODO: Add relevant metadata if needed
            };
            _context.AIBiographies.Add(biography);
            await _context.SaveChangesAsync(cancellationToken);

            // Publish domain event
            biography.AddDomainEvent(new BiographyGeneratedEvent(biography.Id, biography.MemberId, biography.Style, biography.Provider, biography.TokensUsed, biography.UserPrompt));

            return Result<BiographyResultDto>.Success(new BiographyResultDto
            {
                Content = aiResult.Value!.Content,
                Provider = aiResult.Value!.Provider,
                TokensUsed = aiResult.Value!.TokensUsed,
                GeneratedAt = aiResult.Value!.GeneratedAt,
                UserPrompt = request.UserPrompt ?? finalPrompt
            });
        }

        private async Task<string> GeneratePromptFromDBData(Member member, CancellationToken cancellationToken)
        {
            // This is a simplified example. In a real scenario, you would fetch
            // more details about the member (events, relationships, etc.)
            // and construct a rich prompt.
            var prompt = $"Write a biography for {member.FullName} who was born on {member.DateOfBirth?.ToShortDateString()} in {member.PlaceOfBirth}. ";
            if (!string.IsNullOrEmpty(member.Occupation))
            {
                prompt += $"Their occupation was {member.Occupation}. ";
            }
            // Fetch events related to the member
            var events = await _context.Events
                .Where(e => e.RelatedMembers.Any(rm => rm.Id == member.Id))
                .ToListAsync(cancellationToken);

            if (events.Any())
            {
                prompt += "Key life events include: ";
                foreach (var ev in events)
                {
                    prompt += $"{ev.Name} on {ev.StartDate?.ToShortDateString()}. ";
                }
            }

            return prompt;
        }
    }
}
