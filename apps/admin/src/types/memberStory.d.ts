import type { DetectedFace } from './memberFace.d';
import { MemberStoryPerspective, MemberStoryStyle } from './enums';

export interface SearchStoriesFilter { 
  memberId?: string; 
  searchQuery?: string; 
  sortBy?: string; 
  sortOrder?: 'asc' | 'desc'; 
}

export interface MemberStoryDto {
  id?: string;
  memberId: string; 
  memberName?: string | null; 
  title?: string | null;
  story?: string | null;
  detectedFaces?: DetectedFace[]; 
  rawInput?: string | null; 
  storyStyle?: string | null; 
  perspective?: string | null; 
  photo?: string | null; 
  imageSize?: string | null; 
  exifData?: any; 
  targetFaceId?: string | null; 
  memberFullName?: string | null;
  memberAvatarUrl?: string | null;
  memberGender?: string | null;
  originalImageUrl?: string | null;
  resizedImageUrl?: string | null;
  createdAt?: string; 
}

// Matches backend.Application.MemberStories.Commands.CreateMemberStory.CreateMemberStoryCommand
export interface CreateMemberStory {
  memberId: string;
  title: string;
  story: string;
  originalImageUrl?: string | null;
  resizedImageUrl?: string | null;
  rawInput?: string | null; // NEW
  storyStyle?: string | null; // NEW
  perspective?: string | null; // NEW
  detectedFaces: DetectedFace[]; // Using the aligned frontend DTO
}