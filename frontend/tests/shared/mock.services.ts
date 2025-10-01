
import { MockCrudService } from './base.mock.service';
import type { Family, Member, Event } from '@/types';
import { ok } from '@/types';
import { simulateLatency } from '@/utils/mockUtils';
import fixedMockFamilies from '@/data/mock/families.json';
import fixedMockMembers from '@/data/mock/members.json';
import fixedMockEvents from '@/data/mock/events.json';
import type { IFamilyService } from '@/services/family/family.service.interface';
import type { IMemberService } from '@/services/member/member.service.interface';
import type { IEventService } from '@/services/event/event.service.interface';

export class MockFamilyService
  extends MockCrudService<Family>
  implements IFamilyService
{
  constructor() {
    super(JSON.parse(JSON.stringify(fixedMockFamilies)));
  }

  reset() {
    this.items = JSON.parse(JSON.stringify(fixedMockFamilies));
    super.reset();
  }
}

export class MockMemberService
  extends MockCrudService<Member>
  implements IMemberService
{
  constructor() {
    super(JSON.parse(JSON.stringify(fixedMockMembers)));
  }

  reset() {
    this.items = JSON.parse(JSON.stringify(fixedMockMembers));
    super.reset();
  }

  // You can add member-specific mock methods here if needed
  async fetchMembersByFamilyId(familyId: string) {
    const items = this.items.filter((m) => m.familyId === familyId);
    return ok(await simulateLatency(items));
  }
}

export class MockEventService
  extends MockCrudService<Event>
  implements IEventService
{
  constructor() {
    super(JSON.parse(JSON.stringify(fixedMockEvents)));
  }

  reset() {
    this.items = JSON.parse(JSON.stringify(fixedMockEvents));
    super.reset();
  }
}

