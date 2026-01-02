using backend.Application.Common.Models;
using backend.Application.Voice.DTOs;

namespace backend.Application.Common.Interfaces;

public interface IVoiceAIService
{
    Task<Result<VoicePreprocessResponse>> PreprocessVoiceAsync(VoicePreprocessRequest request);
    Task<Result<VoiceGenerateResponse>> GenerateVoiceAsync(VoiceGenerateRequest request);
}