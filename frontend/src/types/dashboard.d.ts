import type { Event as AppEvent } from '@/types';

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
  familyId?: string; // Added for filtering
}

export interface UpcomingEvent extends AppEvent { // Extend existing Event type
  // Add any dashboard-specific properties if needed
}

export interface DashboardData {
  stats: DashboardStats | null;
  recentActivity: RecentActivityItem[];
  upcomingEvents: UpcomingEvent[]; // Changed from upcomingBirthdays
}
