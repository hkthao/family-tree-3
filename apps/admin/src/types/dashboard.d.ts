export enum UserActionType {
  Login = 0,
  Logout = 1,
  CreateFamily = 2,
  UpdateFamily = 3,
  DeleteFamily = 4,
  CreateMember = 5,
  UpdateMember = 6,
  DeleteMember = 7,
  CreateEvent = 8,
  UpdateEvent = 9,
  DeleteEvent = 10,
  CreateRelationship = 11,
  UpdateRelationship = 12,
  DeleteRelationship = 13,
  ChangeRole = 14,
}

export enum TargetType {
  Family = 0,
  Member = 1,
  UserProfile = 2,
  Event = 3,
}

export interface RecentActivity {
  id: string;
  userProfileId: string;
  actionType: UserActionType;
  targetType: TargetType;
  targetId: string;
  activitySummary: string;
  created: string; // ISO date string
  metadata?: any; // Optional JSON metadata
}

export interface DashboardStats {
  totalFamilies: number;
  totalMembers: number;
  totalRelationships: number;
  totalGenerations?: number;
  upcomingEventsCount?: number;
  maleRatio?: number;
  femaleRatio?: number;
  livingMembersCount?: number;
  deceasedMembersCount?: number;
  averageAge?: number;
  membersPerGeneration?: { [key: number]: number };

  // Storage Usage and Limit
  usedStorageMb?: number;
  maxStorageMb?: number;

  // Family Limits
  maxMembers?: number;
  aiChatMonthlyLimit?: number;
  aiChatMonthlyUsage?: number;
}

export interface RecentActivityItem {
  id: string;
  type: 'member' | 'relationship' | 'family';
  description: string;
  timestamp: string;
  familyId?: string;
  // for filtering
}

export interface DashboardData {
  stats: DashboardStats | null;
  upcomingEvents: Event[];
}
