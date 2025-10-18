export interface DashboardStats {
  totalFamilies: number;
  totalMembers: number;
  totalRelationships: number;
  totalGenerations?: number;
}

export interface RecentActivityItem {
  id: string;
  type: 'member' | 'relationship' | 'family';
  description: string;
  timestamp: string;
  familyId?: string;  for filtering
}

export interface DashboardData {
  stats: DashboardStats | null;
  upcomingEvents: Event[];
}
