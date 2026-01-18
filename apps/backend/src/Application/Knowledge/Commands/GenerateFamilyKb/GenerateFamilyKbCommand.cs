using backend.Application.Common.Models;
using MediatR;
using System;

namespace backend.Application.Knowledge.Commands.GenerateFamilyKb;

public class GenerateFamilyKbCommand : IRequest<Result>
{
    public Guid FamilyId { get; set; }
}
