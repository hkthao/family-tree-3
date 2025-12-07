// apps/mobile/family_tree_rn/services/index.ts

import { ApiClientMethods } from '@/types';
import { authService } from '@/services/authService';
import { publicApiClient } from '@/services/api/publicApiClient'; // Import publicApiClient
import { ApiUserProfileService } from '@/services/userProfile/api.userProfile.service';
import { ApiFamilyService } from '@/services/family/api.family.service';
import { IFamilyService } from '@/services/family/family.service.interface';
import { ApiMemberService } from '@/services/member/api.member.service'; // Import ApiMemberService
import { IMemberService } from '@/services/member/member.service.interface'; // Import IMemberService
import { ApiEventService } from '@/services/event/api.event.service'; // Import ApiEventService
import { IEventService } from '@/services/event/event.service.interface'; // Import IEventService
import { ApiRelationshipService } from '@/services/relationship/api.relationship.service'; // Import ApiRelationshipService
import { IRelationshipService } from '@/services/relationship/relationship.service.interface'; // Import IRelationshipService
import { ApiFaceService } from '@/services/face/api.face.service'; // Import ApiFaceService
import { IFaceService } from '@/services/face/face.service.interface'; // Import IFaceService
import { ApiFamilyDictService } from '@/services/familyDict/api.familyDict.service'; // Import ApiFamilyDictService
import { IFamilyDictService } from '@/services/familyDict/familyDict.service.interface'; // Import IFamilyDictService
import { ApiDashboardService } from '@/services/dashboard/api.dashboard.service'; // Import ApiDashboardService
import { IDashboardService } from '@/services/dashboard/dashboard.service.interface'; // Import IDashboardService

// Tạo một apiClient có interceptor để thêm Access Token cho các request cần xác thực
const apiClientWithAuth: ApiClientMethods = {
  get: async <T>(url: string, config?: any) => {
    const accessToken = authService.getAccessToken();
    if (accessToken) {
      publicApiClient.defaults.headers.common['Authorization'] = `Bearer ${accessToken}`;
    }
    const response = await publicApiClient.get<T>(url, config);
    delete publicApiClient.defaults.headers.common['Authorization']; // Xóa header sau khi request hoàn tất
    return response.data;
  },
  post: async <T>(url: string, data?: any, config?: any) => {
    const accessToken = authService.getAccessToken();
    if (accessToken) {
      publicApiClient.defaults.headers.common['Authorization'] = `Bearer ${accessToken}`;
    }
    const response = await publicApiClient.post<T>(url, data, config);
    delete publicApiClient.defaults.headers.common['Authorization'];
    return response.data;
  },
  put: async <T>(url: string, data?: any, config?: any) => {
    const accessToken = authService.getAccessToken();
    if (accessToken) {
      publicApiClient.defaults.headers.common['Authorization'] = `Bearer ${accessToken}`;
    }
    const response = await publicApiClient.put<T>(url, data, config);
    delete publicApiClient.defaults.headers.common['Authorization'];
    return response.data;
  },
  delete: async <T>(url: string, config?: any) => {
    const accessToken = authService.getAccessToken();
    if (accessToken) {
      publicApiClient.defaults.headers.common['Authorization'] = `Bearer ${accessToken}`;
    }
    const response = await publicApiClient.delete<T>(url, config);
    delete publicApiClient.defaults.headers.common['Authorization'];
    return response.data;
  },
};

// Initialize services
export const userProfileService = new ApiUserProfileService(apiClientWithAuth);
export const familyService: IFamilyService = new ApiFamilyService(publicApiClient);
export const memberService: IMemberService = new ApiMemberService(publicApiClient);
export const eventService: IEventService = new ApiEventService(publicApiClient);
export const relationshipService: IRelationshipService = new ApiRelationshipService(publicApiClient);
export const faceService: IFaceService = new ApiFaceService(publicApiClient);
export const familyDictService: IFamilyDictService = new ApiFamilyDictService(publicApiClient);
export const dashboardService: IDashboardService = new ApiDashboardService(publicApiClient);
