// apps/admin/src/types/ai.d.ts

import type { Gender } from '@/types/member'; // Assuming Gender enum is defined here
import type { EventType } from './event'; // Adjusted import path
import type { RelationshipType } from './relationship'; // Adjusted import path


export interface AiPhotoAnalysisInputDto {
  imageUrl?: string | null;
  imageBase64?: string | null;
  imageSize?: string; // e.g., "512x512"
  faces: AiDetectedFaceDto[];
  targetFaceId?: string;
  targetFaceCropUrl?: string | null;
  memberInfo?: AiMemberInfoDto;
  otherFacesSummary?: AiOtherFaceSummaryDto[];
  exif?: AiExifInfoDto;
}

export interface AiDetectedFaceDto {
  faceId: string;
  bbox: number[]; // [x, y, w, h]
  emotionLocal: AiEmotionLocalDto;
  quality?: string;
}

export interface AiEmotionLocalDto {
  dominant: string;
  confidence: number;
}

export interface AiMemberInfoDto {
  id?: string;
  name?: string;
  gender?: Gender; // Assuming Gender is an enum
  age?: number;
}

export interface AiOtherFaceSummaryDto {
  emotionLocal?: string;
}

export interface AiExifInfoDto {
  datetime?: string;
  gps?: string;
  cameraInfo?: string;
  imageDescription?: string;
}

export interface PhotoAnalysisResultDto {
  summary?: string;
  scene?: string;
  event?: string;
  emotion?: string;
  yearEstimate?: string;
  objects?: string[];
  persons?: PhotoAnalysisPersonDto[];
  suggestions?: PhotoAnalysisSuggestionsDto; // NEW PROPERTY
  createdAt: string; // Changed to string as it will be Date on BE but handled as string in TS
}

export interface PhotoAnalysisSuggestionsDto {
  scene: string[];
  event: string[];
  emotion: string[];
}

export interface PhotoAnalysisPersonDto {
  id?: string;
  memberId?: string | null;
  name?: string;
  emotion?: string;
  confidence?: number;
  relationPrompt?: string; // NEW PROPERTY
}

export interface GenerateStoryCommand {
  memberId?: string | null;
  resizedImageUrl?: string | null;
  rawText?: string;
  style?: string; // e.g., nostalgic|warm|formal|folk
  perspective?: string;
}

export interface GenerateStoryResponseDto {
  title: string;
  story: string;
  tags: string[];
  keywords: string[];
  timeline: TimelineEntryDto[];
}

export interface TimelineEntryDto {
  year: number;
  event: string;
}

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