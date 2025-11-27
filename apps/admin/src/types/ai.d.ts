// apps/admin/src/types/ai.d.ts

import type { Gender } from '@/types/member'; // Assuming Gender enum is defined here

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
  photoSummary?: string;
  rawText?: string;
  style?: string; // e.g., nostalgic|warm|formal|folk
  perspective?: string;
  event?: string;
  customEventDescription?: string;
  emotionContexts?: string[];

  // Granular properties from PhotoAnalysisResultDto
  photoSummary?: string;
  photoScene?: string;
  photoEventAnalysis?: string;
  photoEmotionAnalysis?: string;
  photoYearEstimate?: string;
  photoObjects?: string[];
  photoPersons?: PhotoAnalysisPersonDto[];

  maxWords?: number;
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
