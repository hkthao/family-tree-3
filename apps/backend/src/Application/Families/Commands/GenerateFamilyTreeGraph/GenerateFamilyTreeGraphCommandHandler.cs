using Microsoft.Extensions.Logging;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Services.GraphGeneration;
using backend.Domain.Entities;
using backend.Application.Common.Constants; // NEW

namespace backend.Application.Families.Commands.GenerateFamilyTreeGraph;

public class GenerateFamilyTreeGraphCommandHandler : IRequestHandler<GenerateFamilyTreeGraphCommand, Result<GraphGenerationJobDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMessageBus _messageBus;
    private readonly FamilyTreeBuilder _treeBuilder;
    private readonly DotFileGenerator _dotFileGenerator;
    private readonly ILogger<GenerateFamilyTreeGraphCommandHandler> _logger;

    public GenerateFamilyTreeGraphCommandHandler(
        IApplicationDbContext context,
        IMessageBus messageBus,
        FamilyTreeBuilder treeBuilder,
        DotFileGenerator dotFileGenerator,
        ILogger<GenerateFamilyTreeGraphCommandHandler> logger)
    {
        _context = context;
        _messageBus = messageBus;
        _treeBuilder = treeBuilder;
        _dotFileGenerator = dotFileGenerator;
        _logger = logger;
    }

    public async Task<Result<GraphGenerationJobDto>> Handle(GenerateFamilyTreeGraphCommand command, CancellationToken cancellationToken)
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
            return Result<GraphGenerationJobDto>.NotFound($"Không tìm thấy gia đình với ID: {command.FamilyId}");
        }

        var rootMember = family.Members.FirstOrDefault(m => m.Id == command.RootMemberId);
        if (rootMember == null)
        {
            _logger.LogWarning("Không tìm thấy thành viên gốc với RootMemberId: {RootMemberId} trong gia đình {FamilyId}", command.RootMemberId, command.FamilyId);
            return Result<GraphGenerationJobDto>.NotFound($"Không tìm thấy thành viên gốc với ID: {command.RootMemberId} trong gia đình {command.FamilyId}");
        }

        // 2. Build the family sub-tree
        _logger.LogInformation("Xây dựng cây con từ RootMemberId: {RootMemberId}", command.RootMemberId);
        var (nodes, edges) = _treeBuilder.BuildSubTree(family.Members, family.Relationships, command.RootMemberId);

        if (!nodes.Any())
        {
            _logger.LogWarning("Không có node nào được tìm thấy cho cây con từ RootMemberId: {RootMemberId}", command.RootMemberId);
            return Result<GraphGenerationJobDto>.Failure("Không có node nào được tìm thấy để tạo đồ thị.");
        }

        // 3. Generate DOT file content
        _logger.LogInformation("Tạo nội dung file DOT.");
        var dotContent = _dotFileGenerator.GenerateDotFileContent(nodes, edges);

        var outputPath = "/shared/input"; // Đường dẫn cố định như yêu cầu

        // 4. Create and save GraphGenerationJob entity
        // Generate a unique ID first to use it for the filename and job tracking
        var jobId = Guid.NewGuid().ToString("N");
        var dotFilename = $"{jobId}.dot";
        var filePath = Path.Combine(outputPath, dotFilename);

        var job = new GraphGenerationJob
        {
            JobId = jobId,
            FamilyId = command.FamilyId,
            RootMemberId = command.RootMemberId,
            DotFilePath = filePath, // Store the expected path
            Status = "Pending",
            RequestedAt = DateTime.UtcNow
        };
        _context.GraphGenerationJobs.Add(job);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Đã lưu thông tin job tạo đồ thị cây gia phả với JobId: {JobId}", job.JobId);

        // 5. Write the .dot file
        _logger.LogInformation("Ghi file DOT vào: {FilePath}", filePath);
        try
        {
            Directory.CreateDirectory(outputPath); // Đảm bảo thư mục tồn tại
            await File.WriteAllTextAsync(filePath, dotContent, cancellationToken);
            _logger.LogInformation("Ghi file DOT thành công.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi ghi file DOT vào {FilePath}", filePath);
            job.Status = "Failed";
            job.ErrorMessage = $"Lỗi khi ghi file DOT: {ex.Message}";
            await _context.SaveChangesAsync(cancellationToken); // Save the failed status
            return Result<GraphGenerationJobDto>.Failure($"Lỗi khi ghi file DOT: {ex.Message}");
        }

        // 6. Publish RabbitMQ message
        _logger.LogInformation("Gửi tin nhắn RabbitMQ cho JobId: {JobId}", job.JobId);
        var messagePayload = new
        {
            job_id = job.JobId,
            dot_filename = dotFilename,
            page_size = "A0",
            direction = "LR"
        };
        var queueName = MessageBusConstants.Queues.RenderRequestQueue; // Tên queue được yêu cầu
        
        try
        {
            // Serialize to JSON and publish
            await _messageBus.PublishAsync(queueName, queueName, messagePayload, cancellationToken);
            _logger.LogInformation("Tin nhắn RabbitMQ đã được gửi thành công.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi gửi tin nhắn RabbitMQ cho JobId: {JobId}", job.JobId);
            job.Status = "Failed";
            job.ErrorMessage = $"Lỗi khi gửi tin nhắn RabbitMQ: {ex.Message}";
            await _context.SaveChangesAsync(cancellationToken); // Save the failed status
            // Có thể chọn xóa file .dot nếu không gửi được tin nhắn, hoặc để lại để xử lý thủ công.
            // Hiện tại, tôi sẽ không xóa mà chỉ ghi log lỗi.
            return Result<GraphGenerationJobDto>.Failure($"Lỗi khi gửi tin nhắn RabbitMQ: {ex.Message}");
        }

        _logger.LogInformation("Hoàn tất tạo đồ thị cây gia phả và gửi yêu cầu render cho JobId: {JobId}", job.JobId);
        return Result<GraphGenerationJobDto>.Success(new GraphGenerationJobDto
        {
            JobId = job.JobId,
            FamilyId = job.FamilyId,
            RootMemberId = job.RootMemberId,
            Status = job.Status,
            OutputFilePath = job.OutputFilePath,
            ErrorMessage = job.ErrorMessage,
            RequestedAt = job.RequestedAt
        });
    }
}
