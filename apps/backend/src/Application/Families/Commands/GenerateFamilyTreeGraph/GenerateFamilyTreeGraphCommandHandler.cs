using Microsoft.Extensions.Logging;
using backend.Application.Common.Models;
using backend.Application.Services.GraphGeneration;
using backend.Domain.Entities;
using backend.Application.Common.Interfaces.Core;
using backend.Application.Common.Interfaces.Services.GraphvizPdfConverter; // NEW
using backend.Application.Common.Models.GraphvizPdfConverter; // NEW

namespace backend.Application.Families.Commands.GenerateFamilyTreeGraph;

public class GenerateFamilyTreeGraphCommandHandler(
    IApplicationDbContext context,
    FamilyTreeBuilder treeBuilder,
    DotFileGenerator dotFileGenerator,
    ILogger<GenerateFamilyTreeGraphCommandHandler> logger,
    IGraphvizPdfConverterClient graphvizPdfConverterClient) : IRequestHandler<GenerateFamilyTreeGraphCommand, Result<byte[]>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly FamilyTreeBuilder _treeBuilder = treeBuilder;
    private readonly DotFileGenerator _dotFileGenerator = dotFileGenerator;
    private readonly ILogger<GenerateFamilyTreeGraphCommandHandler> _logger = logger;
    private readonly IGraphvizPdfConverterClient _graphvizPdfConverterClient = graphvizPdfConverterClient; // NEW

    public async Task<Result<byte[]>> Handle(GenerateFamilyTreeGraphCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Bắt đầu tạo đồ thị cây gia phả cho FamilyId: {FamilyId}, RootMemberId: {RootMemberId}", command.FamilyId, command.RootMemberId);

        // 1. Fetch Family data including Members and Relationships
        var family = await _context.Families
            .Include(f => f.Members)
            .Include(f => f.Relationships)
            .AsSplitQuery() // Add AsSplitQuery to address EF Core warning
            .FirstOrDefaultAsync(f => f.Id == command.FamilyId, cancellationToken);

        if (family == null)
        {
            _logger.LogWarning("Không tìm thấy gia đình với FamilyId: {FamilyId}", command.FamilyId);
            return Result<byte[]>.NotFound($"Không tìm thấy gia đình với ID: {command.FamilyId}");
        }

        Member? rootMember = null;
        if (command.RootMemberId.HasValue)
        {
            rootMember = family.Members.FirstOrDefault(m => m.Id == command.RootMemberId);
        }

        if (rootMember == null)
        {
            // If RootMemberId is null or not found, try to find a member with IsRoot = true
            rootMember = family.Members.FirstOrDefault(m => m.IsRoot);
        }

        if (rootMember == null)
        {
            _logger.LogWarning("Không tìm thấy thành viên gốc nào. RootMemberId: {RootMemberId} và không có thành viên nào có IsRoot = true trong gia đình {FamilyId}", command.RootMemberId, command.FamilyId);
            return Result<byte[]>.NotFound($"Không tìm thấy thành viên gốc. Vui lòng cung cấp RootMemberId hợp lệ hoặc đảm bảo có ít nhất một thành viên IsRoot = true trong gia đình {command.FamilyId}");
        }

        // 2. Build the family sub-tree
        _logger.LogInformation("Xây dựng cây con từ RootMemberId: {RootMemberId}", rootMember.Id);
        var (nodes, edges, couples) = _treeBuilder.BuildSubTree(family.Members, family.Relationships, rootMember.Id);

        if (!nodes.Any())
        {
            _logger.LogWarning("Không có node nào được tìm thấy cho cây con từ RootMemberId: {RootMemberId}", command.RootMemberId);
            return Result<byte[]>.Failure("Không có node nào được tìm thấy để tạo đồ thị.");
        }

        // 3. Generate DOT file content
        _logger.LogInformation("Tạo nội dung file DOT.");
        var dotContent = _dotFileGenerator.GenerateDotFileContent(nodes, edges, couples, family.Members);

        // 4. Directly convert DOT content to PDF using GraphvizPdfConverterClient
        _logger.LogInformation("Gọi dịch vụ chuyển đổi DOT sang PDF.");
        try
        {
            var request = new ConvertDotToPdfRequest
            {
                DotContent = dotContent,
                PageSize = command.PageSize,
                Direction = command.Direction
            };
            var pdfBytes = await _graphvizPdfConverterClient.ConvertDotToPdfAsync(request, cancellationToken);
            _logger.LogInformation("Chuyển đổi DOT sang PDF thành công.");
            return Result<byte[]>.Success(pdfBytes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi gọi dịch vụ chuyển đổi DOT sang PDF.");
            return Result<byte[]>.Failure($"Lỗi khi chuyển đổi DOT sang PDF: {ex.Message}");
        }
    }
}
