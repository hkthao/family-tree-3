// apps/mobile/family_tree_rn/services/dashboard/api.dashboard.service.ts

import { ApiClientMethods } from '@/types';
import { Result, ApiError, PublicDashboardDto, DashboardMetrics } from '@/types';
import { IDashboardService } from '@/services/dashboard/dashboard.service.interface';

// Helper function from dashboardService.ts
const getGenderColor = (gender: string): string => {
  switch (gender.toLowerCase()) {
    case 'male':
      return '#1E90FF'; // DodgerBlue
    case 'female':
      return '#FF69B4'; // HotPink
    default:
      return '#808080'; // Gray
  }
};

export class ApiDashboardService implements IDashboardService {
  constructor(private api: ApiClientMethods) {}

  async getDashboardData(familyId: string): Promise<Result<DashboardMetrics>> {
    try {
      const data = await this.api.get<PublicDashboardDto>(`/dashboard`, {
        params: {
          familyId: familyId,
        },
      });

      const dashboardMetrics: DashboardMetrics = {
        totalMembers: data.totalPublicMembers,
        totalRelationships: data.totalPublicRelationships,
        totalGenerations: data.totalPublicGenerations,
        averageAge: data.publicAverageAge,
        livingMembers: data.publicLivingMembersCount,
        deceasedMembers: data.publicDeceasedMembersCount,
        genderDistribution: [
          {
            name: 'Male',
            population: Number((data.publicMaleRatio * 100).toFixed(2)), // Convert ratio to percentage and round for display
            color: getGenderColor('male'),
            legendFontColor: '#7F7F7F',
            legendFontSize: 15,
          },
          {
            name: 'Female',
            population: Number((data.publicFemaleRatio * 100).toFixed(2)), // Convert ratio to percentage and round for display
            color: getGenderColor('female'),
            legendFontColor: '#7F7F7F',
            legendFontSize: 15,
          },
        ],
        membersPerGeneration: data.publicMembersPerGeneration,
        totalEvents: data.totalPublicEvents,
      };

      return { isSuccess: true, value: dashboardMetrics };
    } catch (error: any) {
      const apiError: ApiError = {
        message: error.response?.data?.message || error.message || 'An unexpected error occurred.',
        statusCode: error.response?.status,
      };
      return { isSuccess: false, error: apiError };
    }
  }
}
