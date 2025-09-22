import type { IFamilyService } from './family/family.service.interface';
import { MockFamilyService } from './family/mock.family.service';
import { ApiFamilyService } from './family/api.family.service';
import type { IMemberService } from './member/member.service.interface';
import { MockMemberService } from './member/mock.member.service';
import { ApiMemberService } from './member/api.member.service';
import type { IFamilyEventService } from './family-event/family-event.service.interface';
import { MockFamilyEventService } from './family-event/mock.family-event.service';
import { ApiFamilyEventService } from './family-event/api.family-event.service';

export type ServiceMode = 'mock' | 'real' | 'test';

export interface AppServices {
  family: IFamilyService;
  member: IMemberService; 
  familyEvent: IFamilyEventService;
}

export function createServices(mode: ServiceMode, testServices?: Partial<AppServices>): AppServices {
  console.log(`Creating services in ${mode} mode.`);
  return {
    family:
      mode === 'mock'
        ? new MockFamilyService()
        : mode === 'real'
        ? new ApiFamilyService()
        : testServices?.family || new MockFamilyService(), // Use testServices.family if provided
    member:
      mode === 'mock'
        ? new MockMemberService()
        : mode === 'real'
        ? new ApiMemberService()
        : testServices?.member || new MockMemberService(), // Use testServices.member if provided
    familyEvent:
      mode === 'mock'
        ? new MockFamilyEventService()
        : mode === 'real'
        ? new ApiFamilyEventService()
        : testServices?.familyEvent || new MockFamilyEventService(), // Use testServices.familyEvent if provided
  };
}
