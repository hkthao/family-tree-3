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
  isExisting?: boolean; // Added to indicate if the member already exists and needs updating
  loading?: boolean;
  savedSuccessfully?: boolean;
  saveAlert?: { show: boolean; type: 'success' | 'error'; message: string };
}

export interface EventDataDto {
  id?: string | null; // Add id for unique identification in frontend
  type: string;
  description: string;
  date?: string | null;
  location?: string | null;
  relatedMemberIds: string[];
  errorMessage?: string | null;
  loading?: boolean;
  savedSuccessfully?: boolean;
  saveAlert?: { show: boolean; type: 'success' | 'error'; message: string };
}