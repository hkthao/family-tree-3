// apps/admin/src/types/public-face.d.ts

export interface BoundingBoxDto {
  x: number;
  y: number;
  width: number;
  height: number;
}

export interface DetectedFaceDto {
  id: string;
  boundingBox: BoundingBoxDto;
  confidence: number;
  thumbnail?: string;
  embedding?: number[];
  memberId?: string;
  memberName?: string;
  familyId?: string;
  familyName?: string;
  birthYear?: number;
  deathYear?: number;
}

export interface FaceDetectionResponseDto {
  imageId: string;
  detectedFaces: DetectedFaceDto[];
}

export interface DetectFacesRequest {
  imageBytes: string;
  contentType: string;
  returnCrop: boolean;
}
