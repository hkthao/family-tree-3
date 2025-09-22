import type { IFamilyService } from './family/family.service.interface';
import { MockFamilyService } from './family/mock.family.service';
import { ApiFamilyService } from './family/api.family.service';
import type { IMemberService } from './member/member.service.interface';
import { MockMemberService } from './member/mock.member.service';
import { ApiMemberService } from './member/api.member.service';

export type ServiceMode = 'mock' | 'real' | 'test';

export interface AppServices {
  family: IFamilyService;
  member: IMemberService; 
}

export function createServices(mode: ServiceMode, testServices?: Partial<AppServices>): AppServices {
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
  };
}
