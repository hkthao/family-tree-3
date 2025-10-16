import type { UserActionType, TargetType } from '@/types';

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
