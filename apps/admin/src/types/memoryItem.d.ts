import { EmotionalTag, MediaType } from './enums';

export interface MemoryMedia {
  id: string;
  memoryItemId: string;
  url: string;
}

export interface MemoryPerson {
  memberId: string;
  memberName?: string;
}

export interface MemoryItem {
  id: string;
  familyId: string;
  title: string;
  description?: string;
  happenedAt?: Date;
  emotionalTag: EmotionalTag;
  medias: MemoryMedia[];
  persons: MemoryPerson[];
  deletedMediaIds?: string[];
}