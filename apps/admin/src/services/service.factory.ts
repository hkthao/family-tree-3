import type { IFamilyService } from './family/family.service.interface';
import { ApiFamilyService } from './family/api.family.service';
import type { IPublicFamilyService } from './family/public.service.interface'; 
import { PublicApiFamilyService } from './family/publicApi.family.service'; 
import type { IMemberService } from './member/member.service.interface';
import { ApiMemberService } from './member/api.member.service';
import type { IPublicMemberService } from './member/public.service.interface'; 
import { PublicApiMemberService } from './member/publicApi.member.service'; 
import type { IEventService } from './event/event.service.interface';
import { ApiEventService } from './event/api.event.service';
import type { IRelationshipService } from './relationship/relationship.service.interface';
import { ApiRelationshipService } from './relationship/api.relationship.service';
import type { IPublicRelationshipService } from './relationship/public.service.interface'; 
import { PublicApiRelationshipService } from './relationship/publicApi.relationship.service'; 
import type { IDashboardService } from './dashboard/dashboard.service.interface';
import { ApiDashboardService } from './dashboard/api.dashboard.service';
import type { IFileUploadService } from './file-upload/file-upload.service.interface';
import { FileUploadApiService } from './file-upload/api.file-upload.service';
import type { IChatService } from './chat/chat.service.interface';
import { ApiChatService } from './chat/api.chat.service';

import type { IUserService } from './user/user.service.interface';
import { ApiUserService } from './user/api.user.service';
import type { IFamilyDictService } from './family-dict/family-dict.service.interface'; 
import { ApiFamilyDictService } from './family-dict/api.family-dict.service'; 
import type { IMemberStoryService } from './memberStory/memberStory.service.interface'; 
import { ApiMemberStoryService } from './memberStory/api.memberStory.service'; 
import type { IAiService } from './ai/ai.service.interface'; 
import { ApiAiService } from './ai/api.ai.service'; 
import type { IMemberFaceService } from './member-face/member-face.service.interface';
import { ApiMemberFaceService } from './member-face/api.member-face.service'; 
import type { IN8nService } from './n8n/n8n.service.interface'; // NEW IMPORT
import { ApiN8nService } from './n8n/api.n8n.service'; // NEW IMPORT

export type ServiceMode = 'real' | 'test';
export interface AppServices {
  family: IFamilyService;
  publicFamily: IPublicFamilyService; 
  member: IMemberService; 
  publicMember: IPublicMemberService; 
  event: IEventService;
  relationship: IRelationshipService;
  publicRelationship: IPublicRelationshipService; 
  dashboard: IDashboardService;
  fileUpload: IFileUploadService;
  chat: IChatService;

  user: IUserService;
  familyDict: IFamilyDictService; 
  memberStory: IMemberStoryService; 
  ai: IAiService; 
  memberFace: IMemberFaceService; 
  n8n: IN8nService; // NEW SERVICE
}
import apiClient from '@/plugins/axios';
export function createServices(mode: ServiceMode, testServices?: Partial<AppServices>): AppServices {
  return {
    family:
      mode === 'real'
        ? new ApiFamilyService(apiClient)
        : testServices?.family || new ApiFamilyService(apiClient), 
    publicFamily: 
      mode === 'real'
        ? new PublicApiFamilyService(apiClient)
        : testServices?.publicFamily || new PublicApiFamilyService(apiClient),
    member:
      mode === 'real'
        ? new ApiMemberService(apiClient)
        : testServices?.member || new ApiMemberService(apiClient), 
    publicMember: 
      mode === 'real'
        ? new PublicApiMemberService(apiClient)
        : testServices?.publicMember || new PublicApiMemberService(apiClient),
    event:
      mode === 'real'
        ? new ApiEventService(apiClient)
        : testServices?.event || new ApiEventService(apiClient), 
    relationship:
      mode === 'real'
        ? new ApiRelationshipService(apiClient)
        : testServices?.relationship || new ApiRelationshipService(apiClient),
    publicRelationship: 
      mode === 'real'
        ? new PublicApiRelationshipService(apiClient)
        : testServices?.publicRelationship || new PublicApiRelationshipService(apiClient),
    dashboard:
      mode === 'real'
        ? new ApiDashboardService(apiClient)
        : testServices?.dashboard || new ApiDashboardService(apiClient),
    fileUpload:
      mode === 'real'
        ? new FileUploadApiService(apiClient)
        : testServices?.fileUpload || new FileUploadApiService(apiClient),
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
    memberStory: 
      mode === 'real'
        ? new ApiMemberStoryService(apiClient)
        : testServices?.memberStory || new ApiMemberStoryService(apiClient),
    ai: 
      mode === 'real'
        ? new ApiAiService(apiClient)
        : testServices?.ai || new ApiAiService(apiClient),
    memberFace: 
      mode === 'real'
        ? new ApiMemberFaceService(apiClient)
        : testServices?.memberFace || new ApiMemberFaceService(apiClient),
    n8n: // NEW SERVICE INITIALIZATION
      mode === 'real'
        ? new ApiN8nService(apiClient)
        : testServices?.n8n || new ApiN8nService(apiClient),
  };
}

