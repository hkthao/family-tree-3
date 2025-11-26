import type { EventType } from '../event'; // Adjusted import path
import type { RelationshipType } from '../relationship'; // Adjusted import path

export interface AnalyzedDataDto {
  members: MemberDataDto[];
  events: EventDataDto[];
  relationships: RelationshipDataDto[]; // New: Relationships are now separate
  feedback?: string | null;
}

export interface MemberDataDto {
  id?: string | null; // Internal ID (Guid) if existing
  code?: string | null; // Human-readable code if existing and mentioned
  fullName: string;
  firstName?: string | null;
  lastName?: string | null;
  dateOfBirth?: string | null;
  dateOfDeath?: string | null;
  gender?: string | null;
  order?: number | null;
  errorMessage?: string | null;
  isExisting?: boolean; // Added to indicate if the member already exists and needs updating
  loading?: boolean;
  savedSuccessfully?: boolean;
  saveAlert?: { show: boolean; type: 'success' | 'error'; message: string };
}

export interface EventDataDto {
  id?: string | null; // Add id for unique identification in frontend
  type: EventType;
  description: string;
  date?: string | null;
  location?: string | null;
  relatedMemberIds: string[];
  errorMessage?: string | null;
  loading?: boolean;
  savedSuccessfully?: boolean;
  saveAlert?: { show: boolean; type: 'success' | 'error'; message: string };
}

export interface RelationshipDataDto {
  id?: string | null; // For frontend tracking
  sourceMemberId: string;
  targetMemberId: string;
  type: RelationshipType; // e.g., "husband", "wife", "father", "mother"
  order?: number | null;
  errorMessage?: string | null;
  loading?: boolean;
  savedSuccessfully?: boolean;
  saveAlert?: { show: boolean; type: 'success' | 'error'; message: string };
}
