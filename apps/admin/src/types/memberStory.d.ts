import type { DetectedFace } from './memberFace.d';
import { LifeStage } from './enums';
import type { ListOptions } from './pagination.d'; // NEW import

// ------------------------------------
// Request Interfaces
// ------------------------------------

export interface SearchMemberStoriesRequest extends ListOptions {
  memberId?: string;
  familyId?: string;
  searchQuery?: string;
}

// ------------------------------------
// DTOs (Data Transfer Objects)
// ------------------------------------

export interface MemberStoryImageDto {
  id?: string;
  memberStoryId?: string;
  imageUrl?: string | null;
  resizedImageUrl?: string | null;
  caption?: string | null;
}

export interface MemberStoryDto {
  id?: string;
  familyId?: string;
  memberId: string;
  memberName?: string | null;
  title?: string | null;
  story?: string | null;
  year?: number | null;
  timeRangeDescription?: string | null;
  lifeStage?: LifeStage | null;
  location?: string | null;
  storytellerId?: string | null;
  detectedFaces?: DetectedFace[];
  photo?: string | null;
  imageSize?: string | null;
  exifData?: any;
  targetFaceId?: string | null;
  memberFullName?: string | null;
  memberAvatarUrl?: string | null;
  memberGender?: string | null;
  memberStoryImages?: MemberStoryImageDto[]; // Collection of images
  temporaryOriginalImageUrl?: string | null;
  temporaryResizedImageUrl?: string | null;
  createdAt?: string;
}
