// Assuming Member type is already defined elsewhere, e.g., in '@/types/member.d.ts'
// import { Member } from '@/types/member.d.ts';

interface BoundingBox {
  x: number;
  y: number;
  width: number;
  height: number;
}

type FaceStatus = 'recognized' | 'unrecognized' | 'newly-labeled' | 'labeled' | 'original-recognized';

interface DetectedFace {
  id: string; // Unique ID for the detected face instance
  boundingBox: BoundingBox;
  thumbnail: string; // URL to the cropped face image
  memberId: string | null; // ID of the associated member, if recognized/labeled
  originalMemberId?: string | null; // Original ID of the associated member, if recognized initially
  memberName?: string; // For display
  familyId?: string; // For display
  familyName?: string; // For display
  birthYear?: number; // For display
  deathYear?: number; // For display
  embedding: number[] | null; // Embedding vector for the face
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


export {
SearchResult,
FaceMapping,
DetectedFace,
FaceStatus,
BoundingBox,
}