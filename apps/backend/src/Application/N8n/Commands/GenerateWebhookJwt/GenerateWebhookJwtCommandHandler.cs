using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Common.Models.AppSetting;
using MediatR;
using Microsoft.Extensions.Options;

namespace backend.Application.N8n.Commands.GenerateWebhookJwt;

public class GenerateWebhookJwtCommandHandler : IRequestHandler<GenerateWebhookJwtCommand, Result<GenerateWebhookJwtResponse>>
{
    private readonly IJwtService _jwtService;
    private readonly N8nSettings _n8nSettings;

    public GenerateWebhookJwtCommandHandler(IJwtService jwtService, IOptions<N8nSettings> n8nSettings)
    {
        _jwtService = jwtService;
        _n8nSettings = n8nSettings.Value;
    }

    public Task<Result<GenerateWebhookJwtResponse>> Handle(GenerateWebhookJwtCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(_n8nSettings.JwtSecret))
        {
            return Task.FromResult(Result<GenerateWebhookJwtResponse>.Failure("JWT Secret for n8n is not configured."));
        }

        var expires = DateTime.UtcNow.AddMinutes(request.ExpiresInMinutes);
        var token = _jwtService.GenerateToken(request.Subject, expires, _n8nSettings.JwtSecret);

        return Task.FromResult(Result<GenerateWebhookJwtResponse>.Success(new GenerateWebhookJwtResponse
        {
            Token = token,
            ExpiresAt = expires
        }));
    }
}
