export interface Event {
  id: string;
  name: string;
  description?: string;
  startDate: Date | null;
  endDate?: Date | null;
  location?: string;
  familyId: string | null;
  relatedMembers?: string[];
  type: 'Birth' | 'Marriage' | 'Death' | 'Migration' | 'Other';
  color?: string; // Added color property
}
