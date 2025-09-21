export interface Event {
  id: string;
  name: string;
  type: 'Birth' | 'Marriage' | 'Death' | 'Migration' | 'Other';
  familyId: string;
  startDate?: Date | null;
  endDate?: Date | null;
  location?: string;
  description?: string;
  color?: string;
  relatedMembers?: string[]; // Array of member IDs
}

export interface EventFilter {
  name?: string;
  type?: 'Birth' | 'Marriage' | 'Death' | 'Migration' | 'Other';
  familyId?: string | null;
  startDate?: Date | null;
  endDate?: Date | null;
  location?: string;
  relatedMemberId?: string;
}
