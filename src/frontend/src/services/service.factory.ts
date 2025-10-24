import type { IFaceMemberService } from './faceMember/faceMember.service.interface';
import { ApiFaceMemberService } from './faceMember/api.faceMember.service';
import type { IFamilyService } from './family/family.service.interface';
import { ApiFamilyService } from './family/api.family.service';
import type { IMemberService } from './member/member.service.interface';
import { ApiMemberService } from './member/api.member.service';
import type { IEventService } from './event/event.service.interface';
import { ApiEventService } from './event/api.event.service';
import type { IRelationshipService } from './relationship/relationship.service.interface';
import { ApiRelationshipService } from './relationship/api.relationship.service';
import type { IUserProfileService } from './userProfile/user-profile.service.interface';
import { UserProfileApiService } from './userProfile/api.user-profile.service';
import type { IUserActivityService } from './userActivity/user-activity.service.interface';
import { ApiUserActivityService } from './userActivity/api.user-activity.service';
import type { IDashboardService } from './dashboard/dashboard.service.interface';
import { ApiDashboardService } from './dashboard/api.dashboard.service';
import type { IAIBiographyService } from './aiBiography/aiBiography.service.interface';
import { ApiAIBiographyService } from './aiBiography/api.aiBiography.service';
import type { IUserPreferenceService } from './userPreference/user-preference.service.interface';
import { ApiUserPreferenceService } from './userPreference/api.user-preference.service';
import type { IFileUploadService } from './fileUpload/fileUpload.service.interface';
import { FileUploadApiService } from './fileUpload/api.fileUpload.service';
import type { IChatService } from './chat/chat.service.interface';
import { ApiChatService } from './chat/api.chat.service';
import type { INaturalLanguageInputService } from './naturalLanguageInput/naturalLanguageInput.service.interface';
import { ApiNaturalLanguageInputService } from './naturalLanguageInput/api.naturalLanguageInput.service';
import type { IChunkService } from './chunk/chunk.service.interface';
import { ApiChunkService } from './chunk/api.chunk.service';
import type { IFaceService } from './face/face.service.interface';
import { ApiFaceService } from './face/api.face.service';
import type { ISystemConfigService } from './system-config/system-config.service.interface';
import { ApiSystemConfigService } from './system-config/api.system-config.service';
import type { INotificationTemplateService } from './notificationTemplate/notificationTemplate.service.interface';
import { ApiNotificationTemplateService } from './notificationTemplate/api.notificationTemplate.service';

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
  face: IFaceService;
  faceMember: IFaceMemberService;
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
    faceMember:
      mode === 'real'
        ? new ApiFaceMemberService(apiClient)
        : testServices?.faceMember || new ApiFaceMemberService(apiClient),
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