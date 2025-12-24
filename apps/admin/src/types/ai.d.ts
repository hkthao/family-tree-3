export interface CardData {
  id: string;
  type: string;
  title: string;
  summary: string;
}

export interface GenerateAiContentCommand {
  familyId: string;
  chatInput: string;
  contentType: string;
}

export type GenerateAiContentResponse = CardData[];
