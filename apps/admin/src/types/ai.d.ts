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
  memberId?: string;
  name?: string;
  emotion?: string;
  confidence?: number; // NEW PROPERTY
}