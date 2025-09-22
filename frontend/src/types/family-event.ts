export interface FamilyEvent {
  id: string;
  name: string;
  description?: string;
  date: Date; // Assuming date is a Date object
  familyId: string; // Link to the family it belongs to
}
