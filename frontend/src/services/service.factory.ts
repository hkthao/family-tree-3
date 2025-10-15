import type { IFamilyService } from './family/family.service.interface';
import { MockFamilyService } from './family/mock.family.service';
import { ApiFamilyService } from './family/api.family.service';
import type { IMemberService } from './member/member.service.interface';
import { MockMemberService } from './member/mock.member.service';
import { ApiMemberService } from './member/api.member.service';
import type { IEventService } from './event/event.service.interface';
import { MockEventService } from './event/mock.event.service';
import { ApiEventService } from './event/api.event.service';
import type { IRelationshipService } from './relationship/relationship.service.interface';
import { MockRelationshipService } from './relationship/mock.relationship.service';
import { ApiRelationshipService } from './relationship/api.relationship.service';
import type { IUserProfileService } from './userProfile/userProfile.service.interface';
import { MockUserProfileService } from './userProfile/mock.userProfile.service';
import { UserProfileApiService } from './userProfile/api.userProfile.service';
import type { IUserActivityService } from './userActivity/userActivity.service.interface';
import { MockUserActivityService } from './userActivity/mock.userActivity.service';
import { ApiUserActivityService } from './userActivity/api.userActivity.service';
import type { IDashboardService } from './dashboard/dashboard.service.interface';
import { MockDashboardService } from './dashboard/mock.dashboard.service';
import { ApiDashboardService } from './dashboard/api.dashboard.service';
import type { IAIBiographyService } from './aiBiography/aiBiography.service.interface';
import { MockAIBiographyService } from './aiBiography/mock.aiBiography.service';
import { ApiAIBiographyService } from './aiBiography/api.aiBiography.service';
import type { IUserPreferenceService } from './userPreference/userPreference.service.interface';
import { MockUserPreferenceService } from './userPreference/mock.userPreference.service';
import { ApiUserPreferenceService } from './userPreference/api.userPreference.service';
import type { IFileUploadService } from './fileUpload/fileUpload.service.interface';
import { MockFileUploadService } from './fileUpload/mock.fileUpload.service';
import { FileUploadApiService } from './fileUpload/api.fileUpload.service';
import type { IChatService } from './chat/chat.service.interface';
import { ApiChatService } from './chat/api.chat.service';
import { MockChatService } from './chat/mock.chat.service';
import type { INaturalLanguageInputService } from './naturalLanguageInput/naturalLanguageInput.service.interface';
import { ApiNaturalLanguageInputService } from './naturalLanguageInput/api.naturalLanguageInput.service';
import type { IChunkService } from './chunk/chunk.service.interface';
import { ApiChunkService } from './chunk/api.chunk.service';

export type ServiceMode = 'mock' | 'real' | 'test';

export interface AppServices {
  family: IFamilyService;
  member: IMemberService; 
  event: IEventService;
  relationship: IRelationshipService;
  userProfile: IUserProfileService;
  userActivity: IUserActivityService;
  dashboard: IDashboardService;
  aiBiography: IAIBiographyService;
  userPreference: IUserPreferenceService;
  fileUpload: IFileUploadService;
  chat: IChatService;
  naturalLanguageInput: INaturalLanguageInputService;
  chunk: IChunkService;
}

import apiClient from '@/plugins/axios';

export function createServices(mode: ServiceMode, testServices?: Partial<AppServices>): AppServices {
  console.log(`Creating services in ${mode} mode.`);
  return {
    family:
      mode === 'mock'
        ? new MockFamilyService()
        : mode === 'real'
        ? new ApiFamilyService(apiClient)
        : testServices?.family || new MockFamilyService(), // Use testServices.family if provided
    member:
      mode === 'mock'
        ? new MockMemberService()
        : mode === 'real'
        ? new ApiMemberService(apiClient)
        : testServices?.member || new MockMemberService(), // Use testServices.member if provided
    event:
      mode === 'mock'
        ? new MockEventService()
        : mode === 'real'
        ? new ApiEventService(apiClient)
        : testServices?.event || new MockEventService(), // Use testServices.event if provided
    relationship:
      mode === 'mock'
        ? new MockRelationshipService()
        : mode === 'real'
        ? new ApiRelationshipService(apiClient)
        : testServices?.relationship || new MockRelationshipService(),
    userProfile:
      mode === 'mock'
        ? new MockUserProfileService()
        : mode === 'real'
        ? new UserProfileApiService(apiClient)
        : testServices?.userProfile || new MockUserProfileService(),
    userActivity:
      mode === 'mock'
        ? new MockUserActivityService()
        : mode === 'real'
        ? new ApiUserActivityService(apiClient)
        : testServices?.userActivity || new MockUserActivityService(),
    dashboard:
      mode === 'mock'
        ? new MockDashboardService()
        : mode === 'real'
        ? new ApiDashboardService(apiClient)
        : testServices?.dashboard || new MockDashboardService(),
    aiBiography:
      mode === 'mock'
        ? new MockAIBiographyService()
        : mode === 'real'
        ? new ApiAIBiographyService(apiClient)
        : testServices?.aiBiography || new MockAIBiographyService(),
    userPreference:
      mode === 'mock'
        ? new MockUserPreferenceService()
        : mode === 'real'
        ? new ApiUserPreferenceService(apiClient)
        : testServices?.userPreference || new MockUserPreferenceService(),
    fileUpload:
      mode === 'mock'
        ? new MockFileUploadService()
        : mode === 'real'
        ? new FileUploadApiService(apiClient)
        : testServices?.fileUpload || new MockFileUploadService(),
    chat:
      mode === 'mock'
        ? new MockChatService()
        : mode === 'real'
        ? new ApiChatService(apiClient)
        : testServices?.chat || new MockChatService(),
    naturalLanguageInput:
      mode === 'mock'
        ? new ApiNaturalLanguageInputService(apiClient) // Assuming mock is not needed for now, using API service
        : mode === 'real'
        ? new ApiNaturalLanguageInputService(apiClient)
        : testServices?.naturalLanguageInput || new ApiNaturalLanguageInputService(apiClient),
    chunk:
      mode === 'mock'
        ? new ApiChunkService(apiClient) // Using API service for mock mode as well for now
        : mode === 'real'
        ? new ApiChunkService(apiClient)
        : testServices?.chunk || new ApiChunkService(apiClient),
  };
}
