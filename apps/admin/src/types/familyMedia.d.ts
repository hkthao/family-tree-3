import type { BaseAuditableEntity } from './base.d';
import type { RefType, MediaType } from './enums'; // Import from enums.ts

export interface FamilyMedia extends BaseAuditableEntity {
  familyId: string;
  fileName: string;
  filePath: string; // URL to the stored media file
  mediaType: MediaType;
  fileSize: number;
  description?: string;
  thumbnailPath?: string; // URL to the thumbnail, if applicable
  uploadedBy?: string; // User ID
}

export interface MediaLink extends BaseAuditableEntity {
  familyMediaId: string;
  familyMedia?: FamilyMedia; // Navigation property (optional for DTOs)
  refType: RefType;
  refId: string;
}

export interface FamilyMediaFilter {
  searchQuery?: string;
  familyId?: string; // Filter by family ID
  refId?: string; // Filter by linked entity ID
  refType?: RefType; // Filter by linked entity type
  mediaType?: MediaType; // Filter by media type
  // Pagination properties will be handled by ListOptions which are part of Paginated
}

export interface FamilyMediaAddFromUrlDto {
  familyId: string;
  url: string;
  fileName: string;
  mediaType?: MediaType;
  description?: string;
}
export interface MediaItem {
  id: string;
  url: string;
  type: string; // e.g., 'image', 'video'
}