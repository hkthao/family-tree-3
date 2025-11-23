using backend.Application.Common.Constants; // Added for PolicyConstants
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers; // Added for PageSizes, Colors, RelativeUnit
using QuestPDF.Infrastructure;
using SkiaSharp;
using Svg.Skia;

namespace backend.Application.Families.Queries.ExportPdf;

public class GetFamilyPdfExportQueryHandler(
    IApplicationDbContext context,
    IAuthorizationService authorizationService, // Fully qualified
    ICurrentUser currentUser)
    : IRequestHandler<GetFamilyPdfExportQuery, Result<ExportedPdfFile>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IAuthorizationService _authorizationService = authorizationService; // Fully qualified
    private readonly ICurrentUser _currentUser = currentUser;

    public async Task<Result<ExportedPdfFile>> Handle(GetFamilyPdfExportQuery request, CancellationToken cancellationToken)
    {
        // 1. Authorization
        var family = await _context.Families
            .FirstOrDefaultAsync(f => f.Id == request.FamilyId, cancellationToken);

        if (family == null)
        {
            return Result<ExportedPdfFile>.Failure($"Family with ID {request.FamilyId} not found.", ErrorSources.NotFound);
        }

        var succeeded = _authorizationService.CanAccessFamily(request.FamilyId); // Changed _currentUser.User to request.User
        if (!succeeded)
        {
            return Result<ExportedPdfFile>.Failure(ErrorMessages.AccessDenied, ErrorSources.Forbidden);
        }

        // 2. Retrieve Family Data (Moved this part to after authorization and removed duplication)
        family = await _context.Families
            .Include(f => f.Members)
            .ThenInclude(m => m.EventMembers) // Corrected from m.Events
            .Include(f => f.Members)
            .ThenInclude(m => m.SourceRelationships) // Added to include relationships
            .Include(f => f.Members)
            .ThenInclude(m => m.TargetRelationships) // Added to include relationships
            .FirstOrDefaultAsync(f => f.Id == request.FamilyId, cancellationToken);

        // This check is already done above. If it passed authorization, family shouldn't be null again here
        // But for safety, leaving it in for now.
        if (family == null)
        {
            return Result<ExportedPdfFile>.Failure($"Family with ID {request.FamilyId} not found after re-fetching.", ErrorSources.NotFound);
        }

        // 3. Generate PDF Content using QuestPDF
        DocumentMetadata metadata = DocumentMetadata.Default;
        metadata.Title = $"Family Tree: {family.Name}";
        metadata.Author = _currentUser.UserId.ToString() ?? "Family Tree Application";

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(30);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(12));

                page.Header()
                    .Text($"Family Tree: {family.Name}")
                    .SemiBold().FontSize(24).FontColor(Colors.Black); // Changed from Colors.Primary.Darken2 to Colors.Black

                page.Content()
                    .PaddingVertical(1, QuestPDF.Infrastructure.Unit.Centimetre) // Changed to Unit.Centimetre
                    .Column(x =>
                    {
                        x.Spacing(20);

                        x.Item().Text("Family Overview").SemiBold().FontSize(18).Underline();
                        x.Item().Text($"Family Name: {family.Name}");
                        x.Item().Text($"Description: {family.Description ?? "N/A"}");
                        x.Item().Text($"Visibility: {family.Visibility}");
                        x.Item().Text($"Root Member: {family.Members.FirstOrDefault(m => m.IsRoot)?.FullName ?? "N/A"}");

                        x.Item().Text("Members").SemiBold().FontSize(18).Underline();
                        x.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                            });

                            table.Header(header =>
                            {
                                header.Cell().BorderBottom(1).PaddingBottom(5).Text("Name").SemiBold();
                                header.Cell().BorderBottom(1).PaddingBottom(5).Text("Gender").SemiBold();
                                header.Cell().BorderBottom(1).PaddingBottom(5).Text("Date of Birth").SemiBold();
                                header.Cell().BorderBottom(1).PaddingBottom(5).Text("Events Count").SemiBold();
                            });

                            foreach (var member in family.Members)
                            {
                                table.Cell().PaddingVertical(2).Text(member.FullName);
                                table.Cell().PaddingVertical(2).Text(member.Gender);
                                table.Cell().PaddingVertical(2).Text(member.DateOfBirth?.ToShortDateString() ?? "N/A");
                                table.Cell().PaddingVertical(2).Text((member.EventMembers?.Count ?? 0).ToString()); // Added null check
                            }
                        });

                        // Placeholder for Family Tree image (SVG to PNG conversion)
                        x.Item().Text("Family Tree Visualization").SemiBold().FontSize(18).Underline();
                        var dummySvgContent = @"<svg width='200' height='200' viewBox='0 0 200 200' xmlns='http://www.w3.org/2000/svg'>
                                                  <rect x='0' y='0' width='200' height='200' fill='#eee'/>
                                                  <circle cx='100' cy='100' r='80' fill='#f00'/>
                                                  <text x='100' y='105' font-family='Arial' font-size='20' fill='#fff' text-anchor='middle'>Family Tree Placeholder</text>
                                                </svg>";
                        var familyTreeImageBytes = ConvertToPng(dummySvgContent, 600, 400); // Specify dimensions for better control
                        if (familyTreeImageBytes.Length > 0)
                        {
                            x.Item().Image(familyTreeImageBytes).FitWidth();
                        }
                        else
                        {
                            x.Item().Text("Family tree visualization could not be generated.").Italic();
                        }

                        // Placeholder for Timeline
                        // x.Item().Text("Timeline of Events").SemiBold().FontSize(18).Underline();
                        // x.Item().Text("Timeline content goes here...");
                    });

                page.Footer()
                    .AlignCenter()
                    .Text(x =>
                    {
                        x.Span("Page ");
                        x.CurrentPageNumber();
                        x.Span(" of ");
                        x.TotalPages();
                    });
            });
        });

        using var stream = new MemoryStream();
        document.GeneratePdf(stream);
        var pdfContent = stream.ToArray();

        var fileName = $"FamilyTree_{family.Name.Replace(" ", "_")}.pdf";
        return Result<ExportedPdfFile>.Success(new ExportedPdfFile(pdfContent, fileName, "application/pdf"));
    }

    private byte[] ConvertToPng(string svgContent, int width = 800, int height = 600)
    {
        if (string.IsNullOrWhiteSpace(svgContent))
        {
            return new byte[0]; // Return empty byte array for empty SVG
        }

        // Changed SKSvg loading
        var svg = new SKSvg();
        svg.FromSvg(svgContent);

        // Add null checks for Picture and CullRect
        if (svg.Picture == null || svg.Picture.CullRect.IsEmpty)
        {
            return new byte[0]; // Return empty byte array if SVG picture is invalid or empty
        }

        float svgWidth = svg.Picture.CullRect.Size.Width;
        float svgHeight = svg.Picture.CullRect.Size.Height;

        // Calculate aspect ratio to maintain image proportions
        float aspectRatio = svgWidth / svgHeight;
        if (width == 0 && height == 0)
        {
            width = (int)svgWidth;
            height = (int)svgHeight;
        }
        else if (width == 0)
        {
            width = (int)(height * aspectRatio);
        }
        else if (height == 0)
        {
            height = (int)(width / aspectRatio);
        }

        using var bitmap = new SKBitmap(width, height);
        using var canvas = new SKCanvas(bitmap);

        // Scale the SVG to fit the bitmap
        float scaleX = width / svgWidth;
        float scaleY = height / svgHeight;
        float scale = Math.Min(scaleX, scaleY);
        canvas.Scale(scale);

        canvas.DrawPicture(svg.Picture);
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100); // 100 is quality
        return data.ToArray();
    }
}