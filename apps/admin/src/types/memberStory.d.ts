import type { DetectedFace } from './face.d';
import { MemberStoryPerspective, MemberStoryStyle } from './enums';

export interface MemberStoryDto {
  id?: string;
  memberId: string; // Changed from string | null
  memberName?: string | null; // Added for convenience in form
  title?: string | null;
  story?: string | null;
  detectedFaces?: DetectedFace[]; // Renamed from faces
  rawInput?: string | null; // Raw input text for AI generation
  storyStyle?: string | null; // Changed from MemberStoryStyle | null
  perspective?: string | null; // Changed from MemberStoryPerspective | null
  photo?: string | null; // Base64 or URL of the uploaded photo for display
  imageSize?: string | null; // e.g., "1920x1080"
  exifData?: any; // e.g., for storing EXIF metadata from photo
  targetFaceId?: string | null; // The ID of the face selected as the primary subject for the story
  memberFullName?: string | null;
  memberAvatarUrl?: string | null;
  memberGender?: string | null;
  originalImageUrl?: string | null;
  resizedImageUrl?: string | null;
  createdAt?: string; // NEW: Add created date
}

