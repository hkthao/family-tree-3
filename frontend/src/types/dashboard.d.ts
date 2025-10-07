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
}

export interface UpcomingBirthday {
  id: string;
  name: string;
  dateOfBirth: string;
  age?: number;
  avatar?: string; // Added for avatars
}

export interface SystemInfo {
  apiStatus: 'online' | 'offline' | 'unknown';
  appVersion: string;
  lastSync?: string; // Added
  serverTime?: string; // Added
  requestSuccessRate?: number; // Added for pie chart
}

export interface DashboardData {
  stats: DashboardStats | null;
  recentActivity: RecentActivityItem[];
  upcomingBirthdays: UpcomingBirthday[];
  systemInfo: SystemInfo | null;
}