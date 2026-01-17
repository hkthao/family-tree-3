using backend.Application.Common.Exceptions;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Families.Queries;
using backend.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace backend.Application.Families.Commands.RecalculateFamilyStats;

/// <summary>
/// Xử lý lệnh tính toán lại tổng số thành viên và tổng số thế hệ cho một gia đình.
/// </summary>
public class RecalculateFamilyStatsCommandHandler : IRequestHandler<RecalculateFamilyStatsCommand, Result<FamilyStatsDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<RecalculateFamilyStatsCommandHandler> _logger;

    public RecalculateFamilyStatsCommandHandler(IApplicationDbContext context, ILogger<RecalculateFamilyStatsCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<FamilyStatsDto>> Handle(RecalculateFamilyStatsCommand request, CancellationToken cancellationToken)
    {
        var family = await _context.Families
            .Include(f => f.Members)
                .ThenInclude(m => m.SourceRelationships)
            .Include(f => f.Members)
                .ThenInclude(m => m.TargetRelationships)
            .FirstOrDefaultAsync(f => f.Id == request.FamilyId, cancellationToken);

        if (family == null)
        {
            _logger.LogWarning("Family with ID {FamilyId} not found for recalculation.", request.FamilyId);
            throw new NotFoundException(nameof(Family), request.FamilyId);
        }

        try
        {
            family.RecalculateTotalMembers();
            family.RecalculateTotalGenerations();

            await _context.SaveChangesAsync(cancellationToken);

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
