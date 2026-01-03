using AutoMapper;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace backend.Application.VoiceProfiles.Commands.ImportVoiceProfiles;

/// <summary>
/// Xử lý lệnh nhập danh sách hồ sơ giọng nói vào một gia đình.
/// </summary>
public class ImportVoiceProfilesCommandHandler : IRequestHandler<ImportVoiceProfilesCommand, Result<Unit>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public ImportVoiceProfilesCommandHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<Unit>> Handle(ImportVoiceProfilesCommand request, CancellationToken cancellationToken)
    {
        // Lấy tất cả thành viên thuộc về FamilyId được cung cấp
        var memberIdsInFamily = await _context.Members
            .Where(m => m.FamilyId == request.FamilyId)
            .Select(m => m.Id)
            .ToListAsync(cancellationToken);

        if (!memberIdsInFamily.Any())
        {
            return Result<Unit>.Failure("Không tìm thấy thành viên nào thuộc gia đình này.");
        }

        foreach (var voiceProfileDto in request.VoiceProfiles)
        {
            // Kiểm tra xem MemberId của voiceProfileDto có thuộc FamilyId không
            if (!memberIdsInFamily.Contains(voiceProfileDto.MemberId))
            {
                return Result<Unit>.Failure($"Hồ sơ giọng nói với MemberId '{voiceProfileDto.MemberId}' không thuộc về FamilyId '{request.FamilyId}'.");
            }

            var existingVoiceProfile = await _context.VoiceProfiles
                .FirstOrDefaultAsync(vp => vp.Id == voiceProfileDto.Id, cancellationToken);

            if (existingVoiceProfile == null)
            {
                // Thêm mới nếu không tồn tại
                var newVoiceProfile = _mapper.Map<VoiceProfile>(voiceProfileDto);
                _context.VoiceProfiles.Add(newVoiceProfile);
            }
            else
            {
                // Cập nhật nếu đã tồn tại
                _mapper.Map(voiceProfileDto, existingVoiceProfile);
                _context.VoiceProfiles.Update(existingVoiceProfile);
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Result<Unit>.Success(Unit.Value);
    }
}
