import { EmotionalTag, MediaType } from './enums';

export interface MemoryMedia {
  id: string;
  memoryItemId: string;
  mediaType: MediaType; // Corrected from MemoryMediaType to MediaType
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

  media: MemoryMedia[];
  persons: MemoryPerson[];

  created: Date;
  createdBy?: string;
  lastModified?: Date;
  lastModifiedBy?: string;
}