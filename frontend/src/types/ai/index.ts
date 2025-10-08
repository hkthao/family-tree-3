
export enum AIProviderType {
  OpenAI = 'OpenAI',
  Gemini = 'Gemini',
  Mock = 'Mock',
}

export enum BiographyStyle {
  Emotional = 0,
  Historical = 1,
  Storytelling = 2,
  Formal = 3,
  Informal = 4,
}

export interface AIProviderDto {
  providerType: AIProviderType;
  name: string;
  isEnabled: boolean;
  dailyUsageLimit: number;
  currentDailyUsage: number;
  maxTokensPerRequest: number;
}

export interface BiographyResultDto {
  content: string;
  provider: AIProviderType;
  tokensUsed: number;
  generatedAt: Date;
  userPrompt: string;
  style: BiographyStyle;
}
