import { EmotionalTag, MediaType } from './enums';

export interface MemoryMedia {
  id: string;
  memoryItemId: string;
  url: string;
  type: MediaType; // Changed from string to MediaType
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
  location?: string;
  locationId?: string; // ADD THIS
  happenedAt?: Date;
  emotionalTag: EmotionalTag;
  memoryMedia: MemoryMedia[];
  memoryPersons: MemoryPerson[];
  personIds: string[];
  deletedMediaIds?: string[];
  isPrivate?: boolean; // Flag to indicate if some properties were hidden due to privacy
}

export type AddMemoryItemDto = Omit<MemoryItem, 'id' | 'memoryMedia' | 'memoryPersons' | 'deletedMediaIds'> & {
  memoryMedia: Omit<MemoryMedia, 'id' | 'memoryItemId'>[];
};
export type UpdateMemoryItemDto = Omit<MemoryItem, 'memoryPersons'> & {
  memoryMedia: Omit<MemoryMedia, 'memoryItemId'>[];
};