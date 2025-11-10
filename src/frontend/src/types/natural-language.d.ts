export interface AnalyzedDataDto {
  members: MemberDataDto[];
  events: EventDataDto[];
  feedback?: string | null;
}

export interface MemberDataDto {
  id?: string | null;
  fullName: string;
  dateOfBirth?: string | null;
  dateOfDeath?: string | null;
  gender?: string | null;
  relationships: RelationshipDataDto[];
}

export interface RelationshipDataDto {
  type: string;
  relatedMember: MemberDataDto;
}

export interface EventDataDto {
  type: string;
  description: string;
  date?: string | null;
  location?: string | null;
  relatedMemberIds: string[];
}