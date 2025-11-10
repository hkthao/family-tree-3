export interface AnalyzedDataDto {
  members: MemberDataDto[];
  events: EventDataDto[];
  feedback?: string | null;
}

export interface MemberDataDto {
  id?: string | null; // Internal ID (Guid) if existing
  code?: string | null; // Human-readable code if existing and mentioned
  fullName: string;
  firstName?: string | null;
  lastName?: string | null;
  dateOfBirth?: string | null;
  dateOfDeath?: string | null;
  gender?: string | null;
  fatherId?: string | null;
  motherId?: string | null;
  husbandId?: string | null;
  wifeId?: string | null;
  order?: number | null;
  errorMessage?: string | null;
}

export interface EventDataDto {
  type: string;
  description: string;
  date?: string | null;
  location?: string | null;
  relatedMemberIds: string[];
  errorMessage?: string | null;
}