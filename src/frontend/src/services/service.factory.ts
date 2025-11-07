import type { IFamilyService } from './family/family.service.interface';
import { ApiFamilyService } from './family/api.family.service';
import type { IMemberService } from './member/member.service.interface';
import { ApiMemberService } from './member/api.member.service';
import type { IEventService } from './event/event.service.interface';
import { ApiEventService } from './event/api.event.service';
import type { IRelationshipService } from './relationship/relationship.service.interface';
import { ApiRelationshipService } from './relationship/api.relationship.service';
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
import type { INaturalLanguageInputService } from './natural-language-input/natural-language-input.service.interface';
import { ApiNaturalLanguageInputService } from './natural-language-input/api.natural-language-input.service';
import type { IFaceService } from './face/face.service.interface';
import { ApiFaceService } from './face/api.face.service';
import type { IUserService } from './user/user.service.interface';
import { ApiUserService } from './user/api.user.service';


export type ServiceMode = 'real' | 'test';

export interface AppServices {
  family: IFamilyService;
  member: IMemberService; 
  event: IEventService;
  relationship: IRelationshipService;
  userProfile: ICurrentUserProfileService;
  userActivity: ICurrentUserActivityService;
  dashboard: IDashboardService;
  aiBiography: IAIBiographyService;
  userPreference: ICurrentUserPreferenceService;
  fileUpload: IFileUploadService;
  chat: IChatService;
  naturalLanguageInput: INaturalLanguageInputService;
  face: IFaceService;
  user: IUserService;

}

import apiClient from '@/plugins/axios';

export function createServices(mode: ServiceMode, testServices?: Partial<AppServices>): AppServices {
  console.log(`Creating services in ${mode} mode.`);
  return {
    family:
      mode === 'real'
        ? new ApiFamilyService(apiClient)
        : testServices?.family || new ApiFamilyService(apiClient), // Use testServices.family if provided
    member:
      mode === 'real'
        ? new ApiMemberService(apiClient)
        : testServices?.member || new ApiMemberService(apiClient), // Use testServices.member if provided
    event:
      mode === 'real'
        ? new ApiEventService(apiClient)
        : testServices?.event || new ApiEventService(apiClient), // Use testServices.event if provided
    relationship:
      mode === 'real'
        ? new ApiRelationshipService(apiClient)
        : testServices?.relationship || new ApiRelationshipService(apiClient),
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
    naturalLanguageInput:
      mode === 'real'
        ? new ApiNaturalLanguageInputService(apiClient)
        : testServices?.naturalLanguageInput || new ApiNaturalLanguageInputService(apiClient),
    face:
      mode === 'real'
        ? new ApiFaceService(apiClient)
        : testServices?.face || new ApiFaceService(apiClient),
    user:
      mode === 'real'
        ? new ApiUserService(apiClient)
        : testServices?.user || new ApiUserService(apiClient),

  };
}