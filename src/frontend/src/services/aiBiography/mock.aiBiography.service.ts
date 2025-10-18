import type { IAIBiographyService } from './aiBiography.service.interface';
import { ok, type Result } from '@/types';
import { simulateLatency } from '@/utils/mockUtils';
import { AIProviderType, BiographyStyle } from '@/types';
import type { AIBiography, BiographyResultDto, AIProviderDto } from '@/types';

export class MockAIBiographyService implements IAIBiographyService {
  async generateBiography(
    memberId: string,
    style: BiographyStyle,
    generatedFromDB: boolean,
    userPrompt?: string,
    language?: string,
  ): Promise<Result<BiographyResultDto>> {
    console.log('Generating mock biography:', { memberId, style, generatedFromDB, userPrompt, language });
    const generatedContent = `This is a mock biography for member ${memberId} in ${BiographyStyle[style]} style. ` +
                             `Prompt: ${userPrompt || 'Generated from DB data'}. Language: ${language}.`;
    const tokensUsed = generatedContent.length / 5; // Mock token count

    const result: BiographyResultDto = {
      content: generatedContent,
    };
    return simulateLatency(ok(result));
  }


// ... (rest of the file is the same until getLastUserPrompt)

  async getLastAIBiography(memberId: string): Promise<Result<AIBiography | undefined>> {
    console.log('Fetching last mock AI biography for member:', memberId);
    const mockBiography: AIBiography = {
      id: '1',
      memberId: 'member1',
      style: BiographyStyle.Storytelling,
      content: 'This is a mock biography.',
      provider: AIProviderType.Gemini,
      userPrompt: 'Tell me a story.',
      generatedFromDB: false,
      tokensUsed: 100,
      created: new Date().toISOString(),
      language: 'en',  missing language property
    };
    return simulateLatency(ok(mockBiography));
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

  async saveBiography(command: {
    memberId: string;
    style: BiographyStyle;
    content: string;
    provider: AIProviderType;
    userPrompt: string;
    generatedFromDB: boolean;
    tokensUsed: number;
  }): Promise<Result<string>> {
    console.log('Saving mock biography:', command);
    return simulateLatency(ok('mock-saved-biography-id'));
  }
}
