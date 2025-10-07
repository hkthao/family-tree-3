import type { Result } from '@/types/common/result';
import type { RecentActivity, TargetType } from '@/types';
import type { IUserActivityService } from './userActivity.service.interface';
import { ok } from '@/types/common/result';

export class MockUserActivityService implements IUserActivityService {
  private activities: RecentActivity[] = [
    {
      id: '1',
      userProfileId: 'a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11',
      actionType: 0, // Login
      targetType: 2, // UserProfile
      targetId: 'a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11',
      activitySummary: 'User Test User One logged in.',
      created: new Date(Date.now() - 5 * 24 * 60 * 60 * 1000).toISOString(),
    },
    {
      id: '2',
      userProfileId: 'a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11',
      actionType: 2, // CreateFamily
      targetType: 0, // Family
      targetId: '16905e2b-5654-4ed0-b118-bbdd028df6eb',
      activitySummary: 'User Test User One created Royal Family.',
      created: new Date(Date.now() - 4 * 24 * 60 * 60 * 1000).toISOString(),
    },
    {
      id: '3',
      userProfileId: 'b0eebc99-9c0b-4ef8-bb6d-6bb9bd380a22',
      actionType: 0, // Login
      targetType: 2, // UserProfile
      targetId: 'b0eebc99-9c0b-4ef8-bb6d-6bb9bd380a22',
      activitySummary: 'User Test User Two logged in.',
      created: new Date(Date.now() - 3 * 24 * 60 * 60 * 1000).toISOString(),
    },
  ];

  async getRecentActivities(
    limit?: number,
    targetType?: TargetType,
    targetId?: string,
    familyId?: string,
  ): Promise<Result<RecentActivity[]>> {
    await new Promise(resolve => setTimeout(resolve, 500)); // Simulate network delay

    let filteredActivities = this.activities;

    if (targetType !== undefined) {
      filteredActivities = filteredActivities.filter(a => a.targetType === targetType);
    }
    if (targetId) {
      filteredActivities = filteredActivities.filter(a => a.targetId === targetId);
    }
    // familyId filtering would require more complex logic if not directly on activity

    const sortedActivities = filteredActivities.sort((a, b) => new Date(b.created).getTime() - new Date(a.created).getTime());

    return ok(sortedActivities.slice(0, limit || sortedActivities.length));
  }
}
