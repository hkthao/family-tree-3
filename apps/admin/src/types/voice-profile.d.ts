// apps/admin/src/types/voice-profile.d.ts

import type { Paginated } from "./pagination";

export enum VoiceProfileStatus {
  Active = 0,
  Archived = 1,
}

export interface VoiceProfile {
  id: string;
  memberId: string | null;
  label: string;
  audioUrl: string;
  durationSeconds: number;
  language: string;
  consent: boolean;
  status: VoiceProfileStatus;
  created: string;
  createdBy?: string;
  lastModified?: string;
  lastModifiedBy?: string;
}

export interface CreateVoiceProfileCommand {
  memberId: string;
  label: string;
  audioUrl: string;
  durationSeconds: number;
  language: string;
  consent: boolean;
}

export interface PreprocessAndCreateVoiceProfileDto {
  memberId: string;
  label: string;
  rawAudioUrls: string[];
  language: string;
  consent: boolean;
}

export interface UpdateVoiceProfileCommand {
  id: string;
  label: string;
  audioUrl: string;
  durationSeconds: number;
  language: string;
  consent: boolean;
  status: VoiceProfileStatus;
}

export interface VoiceProfileDto extends VoiceProfile {}

export interface VoiceProfilePaginatedResponse extends Paginated<VoiceProfile> {}

