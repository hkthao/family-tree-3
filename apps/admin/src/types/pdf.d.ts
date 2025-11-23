export interface MemberPdfExportData {
  id: string;
  fullName: string;
  gender: string;
  dateOfBirth?: string; // Formatted date string
  eventMembersCount: number;
}

export interface FamilyPdfExportData {
  id: string;
  name: string;
  description?: string;
  members: MemberPdfExportData[];
}