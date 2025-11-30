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
import type { IPublicRelationshipService } from './relationship/public.service.interface'; // New
import { PublicApiRelationshipService } from './relationship/publicApi.relationship.service'; // New
// import type { ICurrentUserProfileService } from './user-profile/user-profile.service.interface'; // REMOVED IMPORT
// import { UserProfileApiService } from './user-profile/api.user-profile.service'; // REMOVED IMPORT
// import type { ICurrentUserActivityService } from './user-activity/user-activity.service.interface'; // REMOVED IMPORT
// import { ApICurrentUserActivityService } from './user-activity/api.user-activity.service'; // REMOVED IMPORT
import type { IDashboardService } from './dashboard/dashboard.service.interface';
import { ApiDashboardService } from './dashboard/api.dashboard.service';
// import type { IAIBiographyService } from './ai-biography/ai-biography.service.interface'; // REMOVED IMPORT
// import { ApiAIBiographyService } from './ai-biography/api.ai-biography.service'; // REMOVED IMPORT
// import type { ICurrentUserPreferenceService } from './user-preference/user-preference.service.interface'; // REMOVED IMPORT
// import { ApICurrentUserPreferenceService } from './user-preference/api.user-preference.service'; // REMOVED IMPORT
import type { IFileUploadService } from './file-upload/file-upload.service.interface';
import { FileUploadApiService } from './file-upload/api.file-upload.service';
import type { IChatService } from './chat/chat.service.interface';
import { ApiChatService } from './chat/api.chat.service';
import type { IFaceService } from './face/face.service.interface';
import { ApiFaceService } from './face/api.face.service';
import type { IUserService } from './user/user.service.interface';
import { ApiUserService } from './user/api.user.service';
// import type { IFamilyDataService } from './family-data/family-data.service.interface'; // REMOVED IMPORT
// import { ApiFamilyDataService } from './family-data/api.family-data.service'; // REMOVED IMPORT
// import type { IPrivacyConfigurationService } from './privacy-configuration/privacy-configuration.service.interface'; // REMOVED IMPORT
// import { ApiPrivacyConfigurationService } from './privacy-configuration/api.privacy-configuration.service'; // REMOVED IMPORT
import type { IFamilyDictService } from './family-dict/family-dict.service.interface'; // Add familyDict service interface
import { ApiFamilyDictService } from './family-dict/api.family-dict.service'; // Add familyDict service implementation
import type { IMemberStoryService } from './memberStory/memberStory.service.interface'; // Updated
import { ApiMemberStoryService } from './memberStory/api.memberStory.service'; // Updated
import type { IAiService } from './ai/ai.service.interface'; // NEW IMPORT
import { ApiAiService } from './ai/api.ai.service'; // NEW IMPORT
import type { IMemberFaceService } from './member-face/member-face.service.interface'; // NEW IMPORT
import { ApiMemberFaceService } from './member-face/api.member-face.service'; // NEW IMPORT


export type ServiceMode = 'real' | 'test';

export interface AppServices {
  family: IFamilyService;
  publicFamily: IPublicFamilyService; // New service
  member: IMemberService; 
  publicMember: IPublicMemberService; // New service
  event: IEventService;
  relationship: IRelationshipService;
  publicRelationship: IPublicRelationshipService; // CORRECTED TYPE
  // userProfile: ICurrentUserProfileService; // REMOVED SERVICE
  // userActivity: ICurrentUserActivityService; // REMOVED SERVICE
  dashboard: IDashboardService;
  // aiBiography: IAIBiographyService; // REMOVED SERVICE
  // userPreference: ICurrentUserPreferenceService; // REMOVED SERVICE
  fileUpload: IFileUploadService;
  chat: IChatService;
  face: IFaceService;
  user: IUserService;
  // naturalLanguage: INaturalLanguageService; // REMOVED SERVICE
  // familyData: IFamilyDataService; // REMOVED SERVICE
  // privacyConfiguration: IPrivacyConfigurationService; // REMOVED SERVICE
  familyDict: IFamilyDictService; // Add familyDict service
  memberStory: IMemberStoryService; // New // Updated
  ai: IAiService; // NEW SERVICE
  memberFace: IMemberFaceService; // NEW SERVICE
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
    // userProfile: // REMOVED SERVICE INSTANTIATION
    //   mode === 'real'
    //     ? new UserProfileApiService(apiClient)
    //     : testServices?.userProfile || new UserProfileApiService(apiClient),
    // userActivity: // REMOVED SERVICE INSTANTIATION
    //   mode === 'real'
    //     ? new ApICurrentUserActivityService(apiClient)
    //     : testServices?.userActivity || new ApICurrentUserActivityService(apiClient),
    dashboard:
      mode === 'real'
        ? new ApiDashboardService(apiClient)
        : testServices?.dashboard || new ApiDashboardService(apiClient),
    // aiBiography: // REMOVED SERVICE INSTANTIATION
    //   mode === 'real'
    //     ? new ApiAIBiographyService(apiClient)
    //     : testServices?.aiBiography || new ApiAIBiographyService(apiClient),
    // userPreference: // REMOVED SERVICE INSTANTIATION
    //   mode === 'real'
    //     ? new ApICurrentUserPreferenceService(apiClient)
    //     : testServices?.userPreference || new ApICurrentUserPreferenceService(apiClient),
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
    // naturalLanguage: // REMOVED SERVICE INSTANTIATION
    //   mode === 'real'
    //     ? new ApiNaturalLanguageService(apiClient)
    //     : testServices?.naturalLanguage || new ApiNaturalLanguageService(apiClient),
    // familyData: // REMOVED SERVICE INSTANTIATION
    //   mode === 'real'
    //     ? new ApiFamilyDataService(apiClient)
    //     : testServices?.familyData || new ApiFamilyDataService(apiClient),
    // privacyConfiguration: // REMOVED SERVICE INSTANTIATION
    //   mode === 'real'
    //     ? new ApiPrivacyConfigurationService(apiClient)
    //     : testServices?.privacyConfiguration || new ApiPrivacyConfigurationService(apiClient),
    familyDict: // Add familyDict service
      mode === 'real'
        ? new ApiFamilyDictService(apiClient)
        : testServices?.familyDict || new ApiFamilyDictService(apiClient),
    memberStory: // New // Updated
      mode === 'real'
        ? new ApiMemberStoryService(apiClient)
        : testServices?.memberStory || new ApiMemberStoryService(apiClient),
    ai: // NEW SERVICE
      mode === 'real'
        ? new ApiAiService(apiClient)
        : testServices?.ai || new ApiAiService(apiClient),
    memberFace: // NEW SERVICE
      mode === 'real'
        ? new ApiMemberFaceService(apiClient)
        : testServices?.memberFace || new ApiMemberFaceService(apiClient),
  };
}





