// apps/mobile/family_tree_rn/src/types/public.d.ts

export enum FamilyRole {
  Manager = 0,
  Viewer = 1,
  Admin = 2,
}

export interface FamilyUserDto {
  familyId: string;
  userId: string;
  userName?: string; // Added
  role: FamilyRole;
}

export interface BaseAuditableDto {
  created: string; // DateTime in C# maps to string in TypeScript
  createdBy?: string;
  lastModified?: string;
  lastModifiedBy?: string;
}

export interface FamilyDetailDto extends BaseAuditableDto {
  id: string;
  name: string;
  code: string;
  description?: string;
  address?: string;
  avatarUrl?: string;
  visibility: string;
  totalMembers: number;
  totalGenerations: number;
  familyUsers: FamilyUserDto[];
}

export interface FamilyListDto extends BaseAuditableDto {
  id: string;
  name: string;
  description?: string;
  address?: string; // Added address property
  avatarUrl?: string; // Added
  totalMembers: number;
  totalGenerations: number; // Added
  visibility: string; // Added
}

export interface PaginatedList<T> {
  items: T[];
  page: number;
  totalPages: number;
  totalItems: number;
}

export interface SearchPublicFamiliesQuery {
  page?: number;
  itemsPerPage?: number;
  searchTerm?: string;
  sortBy?: string;
  sortOrder?: string; // "asc" or "desc"
}

export interface SearchPublicMembersQuery {
  familyId: string;
  page?: number;
  itemsPerPage?: number;
  searchTerm?: string;
  gender?: Gender;
  isRoot?: boolean;
  sortBy?: string;
  sortOrder?: string; // "asc" or "desc"
}

export enum Gender {
  Male = "Male",
  Female = "Female",
  Other = "Other",
}

export interface MemberListDto extends BaseAuditableDto {
  id: string;
  lastName: string;
  firstName: string;
  fullName: string;
  code: string;
  avatarUrl?: string;
  familyId: string;
  familyName?: string;
  isRoot: boolean;
  dateOfBirth?: string; // DateTime in C# maps to string in TypeScript
  dateOfDeath?: string; // DateTime in C# maps to string in TypeScript
  gender?: Gender;
  occupation?: string; // Added
  fatherFullName?: string;
  fatherAvatarUrl?: string;
  motherFullName?: string;
  motherAvatarUrl?: string;
  husbandFullName?: string;
  husbandAvatarUrl?: string;
  wifeFullName?: string;
  wifeAvatarUrl?: string;
  fatherId?: string;
  motherId?: string;
  husbandId?: string;
  wifeId?: string;
  fatherGender?: Gender;
  motherGender?: Gender;
  husbandGender?: Gender;
  wifeGender?: Gender;
  birthDeathYears?: string;
}

export enum RelationshipType {
  Father = "Father",
  Mother = "Mother",
  Husband = "Husband",
  Wife = "Wife",
}

export interface RelationshipMemberDto {
  id: string;
  fullName?: string;
  isRoot: boolean;
  avatarUrl?: string;
  dateOfBirth?: string; // DateTime in C# maps to string in TypeScript
}

export interface RelationshipDto {
  id: string;
  sourceMemberId: string;
  sourceMember?: RelationshipMemberDto;
  targetMemberId: string;
  targetMember?: RelationshipMemberDto;
  type: RelationshipType;
  order?: number;
  familyId: string;
}

export interface MemberDetailDto extends BaseAuditableDto {
  id: string;
  lastName: string;
  firstName: string;
  fullName: string;
  nickname?: string;
  dateOfBirth?: string; // DateTime in C# maps to string in TypeScript
  dateOfDeath?: string; // DateTime in C# maps to string in TypeScript
  placeOfBirth?: string;
  placeOfDeath?: string;
  gender?: Gender;
  avatarUrl?: string;
  occupation?: string;
  email?: string; // Added
  phone?: string; // Added
  address?: string; // Added
  familyId: string;
  biography?: string;
  isRoot: boolean;
  birthDeathYears?: string;
  fatherFullName?: string; // Added
  motherFullName?: string; // Added
  husbandFullName?: string; // Added
  wifeFullName?: string; // Added
  fatherId?: string;
  motherId?: string;
  husbandId?: string;
  wifeId?: string;
  sourceRelationships: RelationshipDto[];
  targetRelationships: RelationshipDto[];
}

export interface RelationshipListDto {
  id: string;
  sourceMemberId: string;
  targetMemberId: string;
  type: RelationshipType;
  order?: number;
  startDate?: string; // DateTime in C# maps to string in TypeScript
  endDate?: string; // DateTime in C# maps to string in TypeScript
  description?: string;
  sourceMember?: RelationshipMemberDto;
  targetMember?: RelationshipMemberDto;
}

export enum EventType {
  /// <summary>
  /// Sự kiện sinh.
  /// </summary>
  Birth = 0,
  /// <summary>
  /// Sự kiện kết hôn.
  /// </summary>
  Marriage = 1,
  /// <summary>
  /// Sự kiện qua đời.
  /// </summary>
  Death = 2,
  /// <summary>
  /// Sự kiện kỷ niệm.
  /// </summary>
  Anniversary = 3,
  /// <summary>
  /// Các loại sự kiện khác.
  /// </summary>
  Other = 4
}

export interface EventDto extends BaseAuditableDto {
  id: string;
  familyId: string;
  name?: string;
  description?: string;
  startDate: string; // DateTime in C# maps to string in TypeScript
  endDate?: string; // DateTime in C# maps to string in TypeScript
  location?: string;
  type: EventType;
  relatedMembers: string[]; // Guid in C# maps to string in TypeScript
}

export interface GetEventsQuery {
  familyId?: string;
  startDate?: string; // DateTime in C# maps to string in TypeScript
  endDate?: string; // DateTime in C# maps to string in TypeScript
  type?: EventType;
  relatedMemberId?: string;
}

export interface SearchPublicEventsQuery {
  familyId?: string;
  searchTerm?: string;
  startDate?: string; // DateTime in C# maps to string in TypeScript
  endDate?: string; // DateTime in C# maps to string in TypeScript
  type?: EventType;
  relatedMemberId?: string;
  page?: number;
  itemsPerPage?: number;
  sortBy?: string;
  sortOrder?: string; // "asc" or "desc"
}

export interface GetPublicUpcomingEventsQuery {
  familyId?: string;
  startDate?: string; // DateTime in C# maps to string in TypeScript
  endDate?: string; // DateTime in C# maps to string in TypeScript
}

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
  thumbnail?: string; // Base64 encoded image
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

// Add FamilyDict types
export enum FamilyDictType {
  Blood = 0,
  Marriage = 1,
  Adoption = 2,
  InLaw = 3,
  Other = 4,
}

export enum FamilyDictLineage {
  Noi = 0,
  Ngoai = 1,
  NoiNgoai = 2,
  Other = 3,
}

export interface NamesByRegionDto {
  north: string;
  central: string | string[];
  south: string | string[];
}

export interface FamilyDictDto {
  id: string;
  name: string;
  type: FamilyDictType;
  description: string;
  lineage: FamilyDictLineage;
  specialRelation: boolean;
  namesByRegion: NamesByRegionDto;
}

export interface FamilyDictFilter {
  searchQuery?: string;
  lineage?: FamilyDictLineage;
  region?: string;
  sortBy?: string;
  sortOrder?: 'asc' | 'desc';
}

export interface PaginatedFamilyDictDto {
  items: FamilyDictDto[];
  page: number;
  totalPages: number;
  totalItems: number;
}
