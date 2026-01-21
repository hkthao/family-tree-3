using backend.Application.Common.Exceptions;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Families.Queries;
using backend.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace backend.Application.Families.Commands.RecalculateFamilyStats;

/// <summary>
/// Xử lý lệnh tính toán lại tổng số thành viên và tổng số thế hệ cho một gia đình.
/// </summary>
public class RecalculateFamilyStatsCommandHandler : IRequestHandler<RecalculateFamilyStatsCommand, Result<FamilyStatsDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<RecalculateFamilyStatsCommandHandler> _logger;
    private readonly IFamilyTreeService _familyTreeService; // NEW: Inject IFamilyTreeService

    public RecalculateFamilyStatsCommandHandler(IApplicationDbContext context, ILogger<RecalculateFamilyStatsCommandHandler> logger, IFamilyTreeService familyTreeService) // NEW: Add IFamilyTreeService
    {
        _context = context;
        _logger = logger;
        _familyTreeService = familyTreeService; // NEW
    }

    public async Task<Result<FamilyStatsDto>> Handle(RecalculateFamilyStatsCommand request, CancellationToken cancellationToken)
    {
        // Fetch family without includes, as stats will be calculated by FamilyTreeService
        var family = await _context.Families
            .FirstOrDefaultAsync(f => f.Id == request.FamilyId, cancellationToken);

        if (family == null)
        {
            _logger.LogWarning("Family with ID {FamilyId} not found for recalculation.", request.FamilyId);
            throw new NotFoundException(nameof(Family), request.FamilyId);
        }

        try
        {
            // Delegate recalculation to the optimized FamilyTreeService
            await _familyTreeService.UpdateFamilyStats(request.FamilyId, cancellationToken);

            // Re-fetch the family or just its stats to get the updated values
            // Alternatively, the FamilyTreeService could return the updated stats directly.
            // For now, re-fetching is simple and ensures consistency.
            family = await _context.Families
                .Where(f => f.Id == request.FamilyId)
                .Select(f => new Family { TotalMembers = f.TotalMembers, TotalGenerations = f.TotalGenerations, Id = f.Id }) // Select only needed properties
                .FirstOrDefaultAsync(cancellationToken);

            if (family == null) // Should not happen if it was found before
            {
                _logger.LogWarning("Family with ID {FamilyId} not found after stats update.", request.FamilyId);
                throw new NotFoundException(nameof(Family), request.FamilyId);
            }

            var familyStats = new FamilyStatsDto
            {
                TotalMembers = family.TotalMembers,
                TotalGenerations = family.TotalGenerations
            };

            _logger.LogInformation("Successfully recalculated stats for Family ID {FamilyId}. TotalMembers: {TotalMembers}, TotalGenerations: {TotalGenerations}",
                request.FamilyId, family.TotalMembers, family.TotalGenerations);

            return Result<FamilyStatsDto>.Success(familyStats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recalculating family stats for Family ID {FamilyId}.", request.FamilyId);
            return Result<FamilyStatsDto>.Failure($"Error recalculating family stats: {ex.Message}");
        }
    }
}
