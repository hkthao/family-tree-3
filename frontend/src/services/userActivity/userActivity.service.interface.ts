import type { Result } from '@/types/common/result';
import type { RecentActivity } from '@/types';
import type { ApiError } from '@/plugins/axios';
import type { TargetType } from '@/types';

export interface IUserActivityService {
  getRecentActivities(
    limit?: number,
    targetType?: TargetType,
    targetId?: string,
    familyId?: string,
  ): Promise<Result<RecentActivity[], ApiError>>;
}
