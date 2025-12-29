import type { Paginated } from '@/types/pagination.d';

interface BoundingBox {
  x: number;
  y: number;
  width: number;
  height: number;
}

type FaceStatus = 'recognized' | 'unrecognized' | 'newly-labeled' | 'labeled';

interface DetectedFace {
  id: string; // Unique ID for the detected face instance
  boundingBox: BoundingBox;
  confidence?: number; // Add confidence property
  thumbnail?: string; // Base64 encoded cropped face image
  thumbnailUrl?: string; // Public URL to the cropped face image
  memberId: string | null; // ID of the associated member, if recognized/labeled
  originalMemberId?: string | null; // Original ID of the associated member, if recognized initially
  memberName?: string; // For display
  familyId?: string; // For display
  familyName?: string; // For display
  birthYear?: number; // For display
  deathYear?: number; // For display
  embedding: number[] | null; // Embedding vector for the face
  emotion?: string;
  emotionConfidence?: number;
  quality?: string;
  relationPrompt?: string; // NEW PROPERTY
  status: FaceStatus; // For UI styling: 'recognized', 'unrecognized', 'newly-labeled', 'original-recognized'
}

interface FaceMapping {
  faceId: string;
  memberId: string;
}

interface SearchResult {
  member: Member; // Full member object or a simplified version
  confidence: number; // Matching confidence (0.0 - 1.0)
}

export interface FaceDetectionResult {
  imageId: string;
  originalImageUrl: string | null;
  resizedImageUrl: string | null;
  detectedFaces: DetectedFace[];
}

export interface MemberFace {
  id: string;
  memberId: string;
  faceId: string;
  boundingBox: BoundingBox;
  confidence?: number;
  thumbnailUrl?: string;
  thumbnail?: string; // NEW: Add thumbnail (base64)
  originalImageUrl?: string | null;
  embedding: number[] | string;
  emotion?: string;
  emotionConfidence?: number;
  isVectorDbSynced: boolean;
  vectorDbId?: string;
  memberName?: string;
  memberGender?: string;
  memberAvatarUrl?: string;
  familyId?: string;
  familyName?: string;
  familyAvatarUrl?: string; // NEW
  isPrivate?: boolean; // Flag to indicate if some properties were hidden due to privacy
}

export type AddMemberFaceDto = CreateMemberFaceCommand;
export type UpdateMemberFaceDto = UpdateMemberFaceCommand;

export interface MemberFaceFilter {  memberId?: string;
  familyId?: string;
  searchQuery?: string;
  emotion?: string; 
  sortBy?: string;
  sortOrder?: 'asc' | 'desc';
}

export interface PaginatedMemberFaces extends Paginated<MemberFace> {}

// ... existing interfaces ...

export interface CreateMemberFaceCommand {
  memberId: string;
  familyId: string;
  faceId: string; // ID from the face detection service
  boundingBox: BoundingBox;
  confidence?: number;
  thumbnail?: string; // Base64 encoded cropped face image
  thumbnailUrl?: string; // Public URL to the cropped face image
  originalImageUrl?: string | null;
  embedding: number[];
  emotion?: string;
  emotionConfidence?: number;
  isVectorDbSynced?: boolean;
  vectorDbId?: string;
}

export interface UpdateMemberFaceCommand {
  id: string; // ID of the member face to update
  memberId: string;
  familyId: string;
  boundingBox?: BoundingBox;
  confidence?: number;
  thumbnail?: string; // Base64 encoded cropped face image
  thumbnailUrl?: string; // Public URL to the cropped face image
  originalImageUrl?: string | null;
  embedding?: number[];
  emotion?: string;
  emotionConfidence?: number;
  isVectorDbSynced?: boolean;
  vectorDbId?: string;
}

// Re-export existing interfaces and types
export {
  SearchResult,
  FaceMapping,
  DetectedFace,
  FaceStatus,
  BoundingBox,
}