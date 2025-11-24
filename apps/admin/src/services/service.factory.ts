import type { IFamilyService } from './family/family.service.interface';
import { ApiFamilyService } from './family/api.family.service';
import type { IPublicFamilyService } from './family/public.service.interface'; // New import
import { PublicApiFamilyService } from './family/publicApi.family.service'; // New import
import type { IMemberService } from './member/member.service.interface';
import { ApiMemberService } from './member/api.member.service';
import type { IPublicMemberService } from './member/public.service.interface'; // New import
import { PublicApiMemberService } from './member/publicApi.member.service'; // New import
import type { IEventService } from './event/event.service.interface';
import { ApiEventService } from './event/api.event.service';
import type { IRelationshipService } from './relationship/relationship.service.interface';
import { ApiRelationshipService } from './relationship/api.relationship.service';
import type { IPublicRelationshipService } from './relationship/public.service.interface';
import { PublicApiRelationshipService } from './relationship/publicApi.relationship.service';
import type { ICurrentUserProfileService } from './user-profile/user-profile.service.interface';
import { UserProfileApiService } from './user-profile/api.user-profile.service';
import type { ICurrentUserActivityService } from './user-activity/user-activity.service.interface';
import { ApICurrentUserActivityService } from './user-activity/api.user-activity.service';
import type { IDashboardService } from './dashboard/dashboard.service.interface';
import { ApiDashboardService } from './dashboard/api.dashboard.service';
import type { IAIBiographyService } from './ai-biography/ai-biography.service.interface';
import { ApiAIBiographyService } from './ai-biography/api.ai-biography.service';
import type { ICurrentUserPreferenceService } from './user-preference/user-preference.service.interface';
import { ApICurrentUserPreferenceService } from './user-preference/api.user-preference.service';
import type { IFileUploadService } from './file-upload/file-upload.service.interface';
import { FileUploadApiService } from './file-upload/api.file-upload.service';
import type { IChatService } from './chat/chat.service.interface';
import { ApiChatService } from './chat/api.chat.service';
import type { IFaceService } from './face/face.service.interface';
import { ApiFaceService } from './face/api.face.service';
import type { IUserService } from './user/user.service.interface';
import { ApiUserService } from './user/api.user.service';
import type { INaturalLanguageService } from './natural-language/api.natural-language.service'; // Import new service interface
import { ApiNaturalLanguageService } from './natural-language/api.natural-language.service'; // Import new service implementation
import type { IFamilyDataService } from './family-data/family-data.service.interface';
import { ApiFamilyDataService } from './family-data/api.family-data.service';
import type { IPrivacyConfigurationService } from './privacy-configuration/privacy-configuration.service.interface';
import { ApiPrivacyConfigurationService } from './privacy-configuration/api.privacy-configuration.service';
import type { IFamilyDictService } from './family-dict/family-dict.service.interface'; // Add familyDict service interface
import { ApiFamilyDictService } from './family-dict/api.family-dict.service'; // Add familyDict service implementation
import type { IMemoryService } from './memory/memory.service.interface'; // New
import { ApiMemoryService } from './memory/api.memory.service'; // New


export type ServiceMode = 'real' | 'test';

export interface AppServices {
  family: IFamilyService;
  publicFamily: IPublicFamilyService; // New service
  member: IMemberService; 
  publicMember: IPublicMemberService; // New service
  event: IEventService;
  relationship: IRelationshipService;
  publicRelationship: IPublicRelationshipService; // New service
  userProfile: ICurrentUserProfileService;
  userActivity: ICurrentUserActivityService;
  dashboard: IDashboardService;
  aiBiography: IAIBiographyService;
  userPreference: ICurrentUserPreferenceService;
  fileUpload: IFileUploadService;
  chat: IChatService;
  face: IFaceService;
  user: IUserService;
  naturalLanguage: INaturalLanguageService; // Add new service to interface
  familyData: IFamilyDataService;
  privacyConfiguration: IPrivacyConfigurationService;
  familyDict: IFamilyDictService; // Add familyDict service
  memory: IMemoryService; // New
}

import apiClient from '@/plugins/axios';

export function createServices(mode: ServiceMode, testServices?: Partial<AppServices>): AppServices {
  
  return {
    family:
      mode === 'real'
        ? new ApiFamilyService(apiClient)
        : testServices?.family || new ApiFamilyService(apiClient), // Use testServices.family if provided
    publicFamily: // New service registration
      mode === 'real'
        ? new PublicApiFamilyService(apiClient)
        : testServices?.publicFamily || new PublicApiFamilyService(apiClient),
    member:
      mode === 'real'
        ? new ApiMemberService(apiClient)
        : testServices?.member || new ApiMemberService(apiClient), // Use testServices.member if provided
    publicMember: // New service registration
      mode === 'real'
        ? new PublicApiMemberService(apiClient)
        : testServices?.publicMember || new PublicApiMemberService(apiClient),
    event:
      mode === 'real'
        ? new ApiEventService(apiClient)
        : testServices?.event || new ApiEventService(apiClient), // Use testServices.event if provided
    relationship:
      mode === 'real'
        ? new ApiRelationshipService(apiClient)
        : testServices?.relationship || new ApiRelationshipService(apiClient),
    publicRelationship: // New service registration
      mode === 'real'
        ? new PublicApiRelationshipService(apiClient)
        : testServices?.publicRelationship || new PublicApiRelationshipService(apiClient),
    userProfile:
      mode === 'real'
        ? new UserProfileApiService(apiClient)
        : testServices?.userProfile || new UserProfileApiService(apiClient),
    userActivity:
      mode === 'real'
        ? new ApICurrentUserActivityService(apiClient)
        : testServices?.userActivity || new ApICurrentUserActivityService(apiClient),
    dashboard:
      mode === 'real'
        ? new ApiDashboardService(apiClient)
        : testServices?.dashboard || new ApiDashboardService(apiClient),
    aiBiography:
      mode === 'real'
        ? new ApiAIBiographyService(apiClient)
        : testServices?.aiBiography || new ApiAIBiographyService(apiClient),
    userPreference:
      mode === 'real'
        ? new ApICurrentUserPreferenceService(apiClient)
        : testServices?.userPreference || new ApICurrentUserPreferenceService(apiClient),
    fileUpload:
      mode === 'real'
        ? new FileUploadApiService(apiClient)
        : testServices?.fileUpload || new FileUploadApiService(apiClient),
    chat:
      mode === 'real'
        ? new ApiChatService(apiClient)
        : testServices?.chat || new ApiChatService(apiClient),
    face:
      mode === 'real'
        ? new ApiFaceService(apiClient)
        : testServices?.face || new ApiFaceService(apiClient),
    user:
      mode === 'real'
        ? new ApiUserService(apiClient)
        : testServices?.user || new ApiUserService(apiClient),
    naturalLanguage: // Register new service
      mode === 'real'
        ? new ApiNaturalLanguageService(apiClient)
        : testServices?.naturalLanguage || new ApiNaturalLanguageService(apiClient),
    familyData:
      mode === 'real'
        ? new ApiFamilyDataService(apiClient)
        : testServices?.familyData || new ApiFamilyDataService(apiClient),
    privacyConfiguration:
      mode === 'real'
        ? new ApiPrivacyConfigurationService(apiClient)
        : testServices?.privacyConfiguration || new ApiPrivacyConfigurationService(apiClient),
    familyDict: // Add familyDict service
      mode === 'real'
        ? new ApiFamilyDictService(apiClient)
        : testServices?.familyDict || new ApiFamilyDictService(apiClient),
    memory: // New
      mode === 'real'
        ? new ApiMemoryService(apiClient)
        : testServices?.memory || new ApiMemoryService(apiClient),
  };
}
