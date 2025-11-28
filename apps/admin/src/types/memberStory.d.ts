import type { DetectedFace } from './detectedFace'; // Assuming DetectedFace is in detectedFace.d.ts

export interface MemberStoryDto {
  id?: string;
  memberId: string | null;
  memberName?: string | null; // Added for convenience in form
  title?: string | null;
  story?: string | null;
  photoUrl?: string | null;
  faces?: DetectedFace[];
  rawInput?: string | null; // Raw input text for AI generation
  storyStyle?: string | null; // Style for AI story generation
  perspective?: string | null; // Perspective for AI story generation
  photo?: string | null; // Base64 or URL of the uploaded photo for display
  imageSize?: string | null; // e.g., "1920x1080"
  exifData?: any; // e.g., for storing EXIF metadata from photo
  targetFaceId?: string | null; // The ID of the face selected as the primary subject for the story
}

