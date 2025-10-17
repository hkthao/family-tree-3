import type { IDashboardService } from './dashboard.service.interface';
import { ok, type Result, type DashboardStats } from '@/types';

export class MockDashboardService implements IDashboardService {
  async fetchStats(familyId?: string): Promise<Result<DashboardStats>> {
    await new Promise(resolve => setTimeout(resolve, 500)); // Simulate network delay
    console.log('Fetching mock stats for family:', familyId);
    return ok({
      totalFamilies: 12,
      totalMembers: 150,
      totalRelationships: 300,
      totalGenerations: 7,
    });
  }
}
