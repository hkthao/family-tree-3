
import { MockCrudService } from './base.mock.service';
import type { Family, Member, Event } from '@/types';
import { ok, type Result } from '@/types';
import type { ApiError } from '@/plugins/axios';
import { simulateLatency } from '@/utils/mockUtils';
import fixedMockFamilies from '@/data/mock/families.json';
import fixedMockMembers from '@/data/mock/members.json';
import type { IFamilyService } from '@/services/family/family.service.interface';
import type { IMemberService } from '@/services/member/member.service.interface';
import fixedMockEvents from '@/data/mock/events.json';
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

  async addItems(newItems: Omit<Family, 'id'>[]): Promise<Result<string[], ApiError>> {
    const newIds: string[] = [];
    newItems.forEach(newItem => {
      const newId = (this.items.length + 1).toString();
      const itemToAdd = { ...newItem, id: newId };
      this.items.push(itemToAdd as Family);
      newIds.push(newId);
    });
    return ok(await simulateLatency(newIds));
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

  async addItems(newItems: Omit<Member, 'id'>[]): Promise<Result<string[], ApiError>> {
    const newIds: string[] = [];
    newItems.forEach(newItem => {
      const newId = (this.items.length + 1).toString();
      const itemToAdd = { ...newItem, id: newId };
      this.items.push(itemToAdd as Member);
      newIds.push(newId);
    });
    return ok(await simulateLatency(newIds));
  }
}

export class MockEventService
  extends MockCrudService<Event & { id: string }>
  implements IEventService
{
  constructor() {
    super(JSON.parse(JSON.stringify(fixedMockEvents)));
  }

  reset() {
    this.items = JSON.parse(JSON.stringify(fixedMockEvents));
    super.reset();
  }

  async getUpcomingEvents(familyId?: string) {
    let events = this.items;
    if (familyId) {
      events = events.filter((event) => event.familyId === familyId);
    }
    return ok(await simulateLatency(events));
  }

  async addMultiple(newItems: Omit<Event, 'id'>[]): Promise<Result<string[], ApiError>> {
    const newIds: string[] = [];
    newItems.forEach(newItem => {
      const newId = (this.items.length + 1).toString();
      const itemToAdd = { ...newItem, id: newId };
      this.items.push(itemToAdd as Event & { id: string });
      newIds.push(newId);
    });
    return ok(await simulateLatency(newIds));
  }
}

