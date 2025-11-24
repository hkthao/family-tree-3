using backend.Application.Common.Models;
using MediatR;
using System;

namespace backend.Application.PdfTemplates.Commands.DeletePdfTemplate;

public record DeletePdfTemplateCommand(Guid Id, Guid FamilyId) : IRequest<Result<Unit>>;