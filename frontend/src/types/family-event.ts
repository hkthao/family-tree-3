export interface FamilyEvent {
  id: string;
  name: string;
  description?: string;
  startDate: Date; // Renamed from date
  endDate?: Date; // Added for consistency with EventList usage
  location?: string; // Added for consistency with EventList usage
  familyId: string; // Link to the family it belongs to
  relatedMembers?: string[]; // Added for consistency with EventList usage (array of member IDs)
}
