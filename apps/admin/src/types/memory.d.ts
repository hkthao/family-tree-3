import type { DetectedFace } from './face';

export interface MemoryDto {
  id?: string;
  memberId: string | null;
  memberName?: string | null;
  title?: string | null;
  story?: string | null;
  photoUrl?: string | null; // URL of the original uploaded photo
  photo?: string | null; // Base64 or URL of the photo (can be resized/processed)
  imageSize?: string | null; // e.g., "1920x1080"
  exifData?: Record<string, any> | null;
  rawInput?: string | null;
  storyStyle?: string | null;
  perspective?: 'firstPerson' | 'neutralPersonal' | 'fullyNeutral' | null;
  faces?: DetectedFace[] | null;
  targetFaceId?: string | null; // ID of the face selected as the primary subject
  uploadedImageId?: string | null; // ID from face recognition service
}
