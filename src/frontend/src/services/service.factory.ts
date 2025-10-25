import type { IFamilyService } from './family/family.service.interface';
import { ApiFamilyService } from './family/api.family.service';
import type { IMemberService } from './member/member.service.interface';
import { ApiMemberService } from './member/api.member.service';
import type { IEventService } from './event/event.service.interface';
import { ApiEventService } from './event/api.event.service';
import type { IRelationshipService } from './relationship/relationship.service.interface';
import { ApiRelationshipService } from './relationship/api.relationship.service';
import type { IUserProfileService } from './user-profile/user-profile.service.interface';
import { UserProfileApiService } from './user-profile/api.user-profile.service';
import type { IUserActivityService } from './user-activity/user-activity.service.interface';
import { ApiUserActivityService } from './user-activity/api.user-activity.service';
import type { IDashboardService } from './dashboard/dashboard.service.interface';
import { ApiDashboardService } from './dashboard/api.dashboard.service';
import type { IAIBiographyService } from './ai-biography/ai-biography.service.interface';
import { ApiAIBiographyService } from './ai-biography/api.ai-biography.service';
import type { IUserPreferenceService } from './user-preference/user-preference.service.interface';
import { ApiUserPreferenceService } from './user-preference/api.user-preference.service';
import type { IFileUploadService } from './file-upload/file-upload.service.interface';
import { FileUploadApiService } from './file-upload/api.file-upload.service';
import type { IChatService } from './chat/chat.service.interface';
import { ApiChatService } from './chat/api.chat.service';
import type { INaturalLanguageInputService } from './natural-language-input/natural-language-input.service.interface';
import { ApiNaturalLanguageInputService } from './natural-language-input/api.natural-language-input.service';
import type { IChunkService } from './chunk/chunk.service.interface';
import { ApiChunkService } from './chunk/api.chunk.service';
import type { IFaceService } from './face/face.service.interface';
import { ApiFaceService } from './face/api.face.service';
import type { ISystemConfigService } from './system-config/system-config.service.interface';
import { ApiSystemConfigService } from './system-config/api.system-config.service';
import type { INotificationTemplateService } from './notification-template/notification-template.service.interface';
import { ApiNotificationTemplateService } from './notification-template/api.notification-template.service';

export type ServiceMode = 'real' | 'test';

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
  face: IFaceService;
  systemConfig: ISystemConfigService;
  notificationTemplate: INotificationTemplateService;
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
        ? new ApiUserActivityService(apiClient)
        : testServices?.userActivity || new ApiUserActivityService(apiClient),
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
        ? new ApiUserPreferenceService(apiClient)
        : testServices?.userPreference || new ApiUserPreferenceService(apiClient),
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
    chunk:
      mode === 'real'
        ? new ApiChunkService(apiClient)
        : testServices?.chunk || new ApiChunkService(apiClient),
    face:
      mode === 'real'
        ? new ApiFaceService(apiClient)
        : testServices?.face || new ApiFaceService(apiClient),
    systemConfig:
      mode === 'real'
        ? new ApiSystemConfigService(apiClient)
        : testServices?.systemConfig || new ApiSystemConfigService(apiClient),
    notificationTemplate:
      mode === 'real'
        ? new ApiNotificationTemplateService(apiClient)
        : testServices?.notificationTemplate || new ApiNotificationTemplateService(apiClient),
  };
}