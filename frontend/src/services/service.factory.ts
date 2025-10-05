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

export type ServiceMode = 'mock' | 'real' | 'test';

export interface AppServices {
  family: IFamilyService;
  member: IMemberService; 
  event: IEventService;
  relationship: IRelationshipService;
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
  };
}
