// apps/admin/src/types/biography.d.ts

export enum BiographyStyle {
  Emotional = 'Emotional',
  Historical = 'Historical',
  Storytelling = 'Storytelling',
  Formal = 'Formal',
  Informal = 'Informal',
}

export interface BiographyResultDto {
  biography?: string;
  style?: BiographyStyle;
  wordCount?: number;
  // Add any other properties from backend's BiographyResultDto
}
