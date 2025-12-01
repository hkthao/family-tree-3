// apps/admin/src/types/public-member.d.ts
import type { BaseAuditableDto } from './common-dto';
import type { Gender } from './member'; // Reusing Gender from admin member types

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
  dateOfBirth?: string;
  dateOfDeath?: string;
  gender?: Gender;
  occupation?: string;
  generation?: number;
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

export interface MemberDetailDto extends BaseAuditableDto {
  id: string;
  lastName: string;
  firstName: string;
  fullName: string;
  nickname?: string;
  dateOfBirth?: string;
  dateOfDeath?: string;
  placeOfBirth?: string;
  placeOfDeath?: string;
  gender?: Gender;
  avatarUrl?: string;
  occupation?: string;
  email?: string;
  phone?: string;
  address?: string;
  familyId: string;
  biography?: string;
  isRoot: boolean;
  birthDeathYears?: string;
  fatherFullName?: string;
  motherFullName?: string;
  husbandFullName?: string;
  wifeFullName?: string;
  fatherId?: string;
  motherId?: string;
  husbandId?: string;
  wifeId?: string;
  sourceRelationships: RelationshipDto[]; // RelationshipDto defined below
  targetRelationships: RelationshipDto[]; // RelationshipDto defined below
}
