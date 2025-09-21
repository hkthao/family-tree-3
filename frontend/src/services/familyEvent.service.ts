import axios from '../plugins/axios';
// eslint-disable-next-line @typescript-eslint/no-unused-vars
import { mockFamilyEvents } from '../data/mock/familyEvents.mock';
import { simulateAsyncOperation } from '../stores/utils';

export interface FamilyEvent {
  id: string;
  familyId: string;
  name: string;
  type: string;
  startDate: string;
  endDate?: string;
  location?: string;
  description?: string;
  color?: string;
  relatedMemberIds?: string[];
  createdAt: string;
  updatedAt: string;
}

export interface FamilyEventServiceType {
  fetchFamilyEvents(search?: string, familyId?: string, page?: number, perPage?: number): Promise<{ items: FamilyEvent[]; total: number }>;
  fetchFamilyEventById(id: string): Promise<FamilyEvent | undefined>;
  addFamilyEvent(event: Omit<FamilyEvent, 'id' | 'createdAt' | 'updatedAt'>): Promise<FamilyEvent>;
  updateFamilyEvent(id: string, event: Partial<Omit<FamilyEvent, 'id' | 'createdAt' | 'updatedAt'>>): Promise<void>;
  removeFamilyEvent(id: string): Promise<void>;
}

export class RealFamilyEventService implements FamilyEventServiceType {
  async fetchFamilyEvents(search?: string, familyId?: string, page?: number, perPage?: number): Promise<{ items: FamilyEvent[]; total: number }> {
    // eslint-disable-next-line @typescript-eslint/no-unused-vars
    const response = await axios.get(`/family-events`, {
      params: { search, familyId, page, perPage },
    });
    return response.data;
  }

  async fetchFamilyEventById(id: string): Promise<FamilyEvent | undefined> {
    // eslint-disable-next-line @typescript-eslint/no-unused-vars
    const response = await axios.get(`/family-events/${id}`);
    return response.data;
  }

  async addFamilyEvent(event: Omit<FamilyEvent, 'id' | 'createdAt' | 'updatedAt'>): Promise<FamilyEvent> {
    // eslint-disable-next-line @typescript-eslint/no-unused-vars
    const response = await axios.post(`/family-events`, event);
    return response.data;
  }

  async updateFamilyEvent(id: string, event: Partial<Omit<FamilyEvent, 'id' | 'createdAt' | 'updatedAt'>>): Promise<void> {
    // eslint-disable-next-line @typescript-eslint/no-unused-vars
    await axios.put(`/family-events/${id}`, event);
  }

  async removeFamilyEvent(id: string): Promise<void> {
    // eslint-disable-next-line @typescript-eslint/no-unused-vars
    await axios.delete(`/family-events/${id}`);
  }
}

export class MockFamilyEventService implements FamilyEventServiceType {
  private events: FamilyEvent[] = [];

  constructor(initialEvents: FamilyEvent[] = []) {
    this.events = JSON.parse(JSON.stringify(initialEvents)); // Deep copy
  }

  async fetchFamilyEvents(search?: string, familyId?: string, page = 1, perPage = 10): Promise<{ items: FamilyEvent[]; total: number }> {
    let filtered = this.events;
    if (familyId) {
      filtered = filtered.filter(e => e.familyId === familyId);
    }
    if (search) {
      filtered = filtered.filter(e => e.name.toLowerCase().includes(search.toLowerCase()));
    }
    const items = filtered.slice((page - 1) * perPage, page * perPage);
    return simulateAsyncOperation({ items, total: filtered.length });
  }

  async fetchFamilyEventById(id: string): Promise<FamilyEvent | undefined> {
    const event = this.events.find(e => e.id === id);
    return simulateAsyncOperation(event);
  }

  async addFamilyEvent(event: Omit<FamilyEvent, 'id' | 'createdAt' | 'updatedAt'>): Promise<FamilyEvent> {
    const newEvent: FamilyEvent = {
      ...event,
      id: 'mock-' + (this.events.length + 1),
      createdAt: new Date().toISOString(),
      updatedAt: new Date().toISOString(),
    };
    this.events.push(newEvent);
    return simulateAsyncOperation(newEvent);
  }

  async updateFamilyEvent(id: string, event: Partial<Omit<FamilyEvent, 'id' | 'createdAt' | 'updatedAt'>>): Promise<void> {
    const index = this.events.findIndex(e => e.id === id);
    if (index !== -1) {
      this.events[index] = { ...this.events[index], ...event, updatedAt: new Date().toISOString() };
    }
    return simulateAsyncOperation(undefined);
  }

  async removeFamilyEvent(id: string): Promise<void> {
    const index = this.events.findIndex(e => e.id === id);
    if (index !== -1) {
      this.events.splice(index, 1);
    }
    return simulateAsyncOperation(undefined);
  }
}
