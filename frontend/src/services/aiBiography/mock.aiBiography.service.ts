import type { IAIBiographyService } from './aiBiography.service.interface';
import type { Result } from '@/types/common/result';
import type { BiographyResultDto, AIProviderDto } from '@/types';
import { ok, err } from '@/types/common/result';
import { simulateLatency } from '@/utils/mockUtils';
import { AIProviderType, BiographyStyle } from '@/types';

export class MockAIBiographyService implements IAIBiographyService {
  async generateBiography(
    memberId: string,
    style: BiographyStyle,
    useDBData: boolean,
    userPrompt?: string,
    language?: string,
  ): Promise<Result<BiographyResultDto>> {
    console.log('Generating mock biography:', { memberId, style, useDBData, userPrompt, language });
    const generatedContent = `This is a mock biography for member ${memberId} in ${BiographyStyle[style]} style. ` +
                             `Prompt: ${userPrompt || 'Generated from DB data'}. Language: ${language}.`;
    const tokensUsed = generatedContent.length / 5; // Mock token count

    const result: BiographyResultDto = {
      content: generatedContent,
      provider: AIProviderType.Gemini,
      tokensUsed: Math.round(tokensUsed),
      generatedAt: new Date(),
      userPrompt: userPrompt || 'Generated from DB data',
    };
    return simulateLatency(ok(result));
  }

  async getLastUserPrompt(memberId: string): Promise<Result<string | undefined>> {
    console.log('Fetching last mock user prompt for member:', memberId);
    // Simulate fetching from cache/storage
    const mockPrompt = 'Write a heroic story about a brave warrior.';
    return simulateLatency(ok(mockPrompt));
  }

  async getAIProviders(): Promise<Result<AIProviderDto[]>> {
    console.log('Fetching mock AI providers');
    const providers: AIProviderDto[] = [
      {
        providerType: AIProviderType.Gemini,
        name: 'Google Gemini (Mock)',
        isEnabled: true,
        dailyUsageLimit: 1000,
        currentDailyUsage: 150,
        maxTokensPerRequest: 500,
      },
      {
        providerType: AIProviderType.OpenAI,
        name: 'OpenAI (Mock)',
        isEnabled: true,
        dailyUsageLimit: 1000,
        currentDailyUsage: 200,
        maxTokensPerRequest: 500,
      },
    ];
    return simulateLatency(ok(providers));
  }
}
