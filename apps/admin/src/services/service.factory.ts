import type { IFamilyService } from './family/family.service.interface';
import { ApiFamilyService } from './family/api.family.service';
import type { IMemberService } from './member/member.service.interface';
import { ApiMemberService } from './member/api.member.service';
import { ApiEventService } from './event/api.event.service';
import type { IRelationshipService } from './relationship/relationship.service.interface';
import { ApiRelationshipService } from './relationship/api.relationship.service';
import type { IDashboardService } from './dashboard/dashboard.service.interface';
import { ApiDashboardService } from './dashboard/api.dashboard.service';
import type { IChatService } from './chat/chat.service.interface';
import { ApiChatService } from './chat/api.chat.service';
import type { IUserService } from './user/user.service.interface';
import { ApiUserService } from './user/api.user.service';
import type { IFamilyDictService } from './family-dict/family-dict.service.interface';
import { ApiFamilyDictService } from './family-dict/api.family-dict.service';
import type { IMemberFaceService } from './member-face/member-face.service.interface';
import { ApiMemberFaceService } from './member-face/api.member-face.service';
import type { IPromptService } from './prompt/prompt.service.interface';
import { ApiPromptService } from './prompt/api.prompt.service';
import type { IFamilyLinkService } from './familyLink/familyLink.service.interface';
import { ApiFamilyLinkService } from './familyLink/api.familyLink.service';
import type { IFamilyLinkRequestService } from './familyLinkRequest/familyLinkRequest.service.interface';
import { ApiFamilyLinkRequestService } from './familyLinkRequest/api.familyLinkRequest.service'; // ADD THIS IMPORT
import type { IFamilyMediaService } from './family-media/family-media.service.interface';
import { ApiFamilyMediaService } from './family-media/api.family-media.service';
import type { IFamilyLocationService } from './family-location/family-location.service.interface';
import { ApiFamilyLocationService } from './family-location/api.family-location.service';
import type { IMemoryItemService } from './memory-item/memory-item.service.interface';
import { ApiMemoryItemService } from './memory-item/api.memory-item.service';
import type { IImageRestorationJobService } from './image-restoration-job/image-restoration-job.service.interface'; // NEW
import { ApiImageRestorationJobService } from './image-restoration-job/api.image-restoration-job.service'; // NEW
import type { IVoiceProfileService } from './voice-profile/voice-profile.service.interface';
import { ApiVoiceProfileService } from './voice-profile/api.voice-profile.service';

export type ServiceMode = 'real' | 'test';
export interface AppServices {
  family: IFamilyService;
  member: IMemberService;
  event: IEventService;
  relationship: IRelationshipService;
  dashboard: IDashboardService;
  chat: IChatService;
  user: IUserService;
  familyDict: IFamilyDictService;
  memberFace: IMemberFaceService;
  prompt: IPromptService;
  familyLink: IFamilyLinkService;
  familyLinkRequest: IFamilyLinkRequestService;
  familyMedia: IFamilyMediaService; // NEW: Add FamilyMediaService
  familyLocation: IFamilyLocationService; // NEW: Add FamilyLocationService
  memoryItem: IMemoryItemService; // NEW: Add MemoryItemService
  imageRestorationJob: IImageRestorationJobService; // NEW
  voiceProfile: IVoiceProfileService;
}
import apiClient from '@/plugins/axios';
import type { IEventService } from './event/event.service.interface';
export function createServices(mode: ServiceMode, testServices?: Partial<AppServices>): AppServices {
  return {
    family:
      mode === 'real'
        ? new ApiFamilyService(apiClient)
        : testServices?.family || new ApiFamilyService(apiClient),
    member:
      mode === 'real'
        ? new ApiMemberService(apiClient)
        : testServices?.member || new ApiMemberService(apiClient),
    event:
      mode === 'real'
        ? new ApiEventService(apiClient)
        : testServices?.event || new ApiEventService(apiClient),
    relationship:
      mode === 'real'
        ? new ApiRelationshipService(apiClient)
        : testServices?.relationship || new ApiRelationshipService(apiClient),
    dashboard:
      mode === 'real'
        ? new ApiDashboardService(apiClient)
        : testServices?.dashboard || new ApiDashboardService(apiClient),
    chat:
      mode === 'real'
        ? new ApiChatService(apiClient)
        : testServices?.chat || new ApiChatService(apiClient),

    user:
      mode === 'real'
        ? new ApiUserService(apiClient)
        : testServices?.user || new ApiUserService(apiClient),
    familyDict:
      mode === 'real'
        ? new ApiFamilyDictService(apiClient)
        : testServices?.familyDict || new ApiFamilyDictService(apiClient),
    memberFace:
      mode === 'real'
        ? new ApiMemberFaceService(apiClient)
        : testServices?.memberFace || new ApiMemberFaceService(apiClient),
    prompt:
      mode === 'real'
        ? new ApiPromptService(apiClient)
        : testServices?.prompt || new ApiPromptService(apiClient),
    familyLink:
      mode === 'real'
        ? new ApiFamilyLinkService(apiClient)
        : testServices?.familyLink || new ApiFamilyLinkService(apiClient),
    familyLinkRequest:
      mode === 'real'
        ? new ApiFamilyLinkRequestService(apiClient)
        : testServices?.familyLinkRequest || new ApiFamilyLinkRequestService(apiClient),
    familyMedia: // NEW: Assign FamilyMediaService
      mode === 'real'
        ? new ApiFamilyMediaService(apiClient)
        : testServices?.familyMedia || new ApiFamilyMediaService(apiClient),
    familyLocation: // NEW: Assign FamilyLocationService
      mode === 'real'
        ? new ApiFamilyLocationService(apiClient)
        : testServices?.familyLocation || new ApiFamilyLocationService(apiClient),
    memoryItem: // NEW: Assign MemoryItemService
      mode === 'real'
        ? new ApiMemoryItemService(apiClient)
        : testServices?.memoryItem || new ApiMemoryItemService(apiClient),
    imageRestorationJob: // NEW: Assign ImageRestorationJobService
      mode === 'real'
        ? new ApiImageRestorationJobService(apiClient)
        : testServices?.imageRestorationJob || new ApiImageRestorationJobService(apiClient),
    voiceProfile:
      mode === 'real'
        ? new ApiVoiceProfileService(apiClient)
        : testServices?.voiceProfile || new ApiVoiceProfileService(apiClient),
  };
}