
export enum AIProviderType {
  OpenAI = 'OpenAI',
  Gemini = 'Gemini',
  Mock = 'Mock',
}

export enum BiographyStyle {
  Professional = 'Professional',
  Emotional = 'Emotional',
  Humorous = 'Humorous',
  Factual = 'Factual',
  Historical = 'Historical',
  Storytelling = 'Storytelling',
  Formal = 'Formal',
  Informal = 'Informal',
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
