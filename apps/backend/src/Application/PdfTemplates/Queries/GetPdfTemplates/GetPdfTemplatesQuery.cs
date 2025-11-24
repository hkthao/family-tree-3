using backend.Application.Common.Models;
using backend.Application.PdfTemplates.Dtos;
using MediatR;
using System;
using System.Collections.Generic;

namespace backend.Application.PdfTemplates.Queries.GetPdfTemplates;

public record GetPdfTemplatesQuery(Guid FamilyId) : IRequest<Result<List<PdfTemplateDto>>>;