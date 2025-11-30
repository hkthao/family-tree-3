import type { Paginated } from '@/types/pagination.d';
import type { Result } from '@/types/result.d';
import type { ApiError } from '@/plugins/axios';
import type { BoundingBox } from '@/types/face.d'; // Import BoundingBox from face.d

export interface MemberFace {
  id: string; // Guid on backend is string on frontend
  memberId: string;
  faceId: string;
  boundingBox: BoundingBox;
  confidence: number;
  thumbnailUrl?: string;
  originalImageUrl?: string;
  embedding: number[]; // List<double> on backend is number[] on frontend
  emotion?: string;
  emotionConfidence?: number;
  isVectorDbSynced: boolean;
  vectorDbId?: string;

  // Enriched data
  memberName?: string;
  familyId?: string;
  familyName?: string;
}

export interface MemberFaceFilter {
  memberId?: string;
  familyId?: string;
  searchQuery?: string; // Add if search functionality is needed
  sortBy?: string;
  sortOrder?: 'asc' | 'desc';
}

export interface PaginatedMemberFaces extends Paginated<MemberFace> {}

