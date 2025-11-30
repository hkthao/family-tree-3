import type { Paginated } from '@/types/pagination.d';
import type { Result } from '@/types/result.d';
import type { ApiError } from '@/plugins/axios';
import type { BoundingBox } from '@/types/face.d'; 

export interface MemberFace {
  id: string; 
  memberId: string;
  faceId: string;
  boundingBox: BoundingBox;
  confidence?: number;
  thumbnailUrl?: string;
  originalImageUrl?: string | null;
  embedding: number[]; 
  emotion?: string;
  emotionConfidence?: number;
  isVectorDbSynced: boolean;
  vectorDbId?: string;

  
  memberName?: string;
  memberGender?: string; 
  memberAvatarUrl?: string; 
  familyId?: string;
  familyName?: string;
}

export interface MemberFaceFilter {
  memberId?: string;
  familyId?: string;
  searchQuery?: string; 
  sortBy?: string;
  sortOrder?: 'asc' | 'desc';
}

export interface PaginatedMemberFaces extends Paginated<MemberFace> {}

