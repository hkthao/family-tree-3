import axios from '../plugins/axios';
// eslint-disable-next-line @typescript-eslint/no-unused-vars
import { mockFamilies } from '../data/mock/families.mock';
import { simulateAsyncOperation } from '../stores/utils';

export interface Family {
  id: string;
  name: string;
  description?: string;
  address?: string;
  avatarUrl?: string;
  visibility: 'Private' | 'Public';
  createdAt: string;
  updatedAt: string;
}

export interface FamilyServiceType {
  fetchFamilies(search?: string, page?: number, perPage?: number): Promise<{ items: Family[]; total: number }>;
  fetchFamilyById(id: string): Promise<Family | undefined>;
  addFamily(family: Omit<Family, 'id' | 'createdAt' | 'updatedAt'>): Promise<Family>;
  updateFamily(id: string, family: Partial<Omit<Family, 'id' | 'createdAt' | 'updatedAt'>>): Promise<void>;
  removeFamily(id: string): Promise<void>;
}

export class RealFamilyService implements FamilyServiceType {
  fetchFamilies(search?: string, page?: number, perPage?: number): Promise<{ items: Family[]; total: number }>;
  fetchFamilyById(id: string): Promise<Family | undefined>;
  addFamily(family: Omit<Family, 'id' | 'createdAt' | 'updatedAt'>): Promise<Family>;
  updateFamily(id: string, family: Partial<Omit<Family, 'id' | 'createdAt' | 'updatedAt'>>): Promise<void>;
  removeFamily(id: string): Promise<void>;
}

export class RealFamilyService implements FamilyServiceType {
  async fetchFamilies(search?: string, page?: number, perPage?: number): Promise<{ items: Family[]; total: number }> {
    // eslint-disable-next-line @typescript-eslint/no-unused-vars
    const response = await axios.get(`/families`, {
      params: { search, page, perPage },
    });
    return response.data;
  }

  async fetchFamilyById(id: string): Promise<Family | undefined> {
    // eslint-disable-next-line @typescript-eslint/no-unused-vars
    const response = await axios.get(`/families/${id}`);
    return response.data;
  }

  async addFamily(family: Omit<Family, 'id' | 'createdAt' | 'updatedAt'>): Promise<Family> {
    // eslint-disable-next-line @typescript-eslint/no-unused-vars
    const response = await axios.post(`/families`, family);
    return response.data;
  }

  async updateFamily(id: string, family: Partial<Omit<Family, 'id' | 'createdAt' | 'updatedAt'>>): Promise<void> {
    // eslint-disable-next-line @typescript-eslint/no-unused-vars
    await axios.put(`/families/${id}`, family);
  }

  async removeFamily(id: string): Promise<void> {
    // eslint-disable-next-line @typescript-eslint/no-unused-vars
    await axios.delete(`/families/${id}`);
  }
}

export class MockFamilyService implements FamilyServiceType {
  private families: Family[] = [];

  constructor(initialFamilies: Family[] = []) {
    this.families = JSON.parse(JSON.stringify(initialFamilies)); // Deep copy
  }

  async fetchFamilies(search?: string, page = 1, perPage = 10): Promise<{ items: Family[]; total: number }> {
    let filtered = this.families;
    if (search) {
      filtered = filtered.filter(f => f.name.toLowerCase().includes(search.toLowerCase()));
    }
    const items = filtered.slice((page - 1) * perPage, page * perPage);
    return simulateAsyncOperation({ items, total: filtered.length });
  }

  async fetchFamilyById(id: string): Promise<Family | undefined> {
    const family = this.families.find(f => f.id === id);
    return simulateAsyncOperation(family);
  }

  async addFamily(family: Omit<Family, 'id' | 'createdAt' | 'updatedAt'>): Promise<Family> {
    const newFamily: Family = {
      ...family,
      id: 'mock-' + (this.families.length + 1),
      createdAt: new Date().toISOString(),
      updatedAt: new Date().toISOString(),
      avatarUrl: 'https://via.placeholder.com/150', // Default avatar for new mock families
    };
    this.families.push(newFamily);
    return simulateAsyncOperation(newFamily);
  }

  async updateFamily(id: string, family: Partial<Omit<Family, 'id' | 'createdAt' | 'updatedAt'>>): Promise<void> {
    const index = this.families.findIndex(f => f.id === id);
    if (index !== -1) {
      this.families[index] = { ...this.families[index], ...family, updatedAt: new Date().toISOString() };
    }
    return simulateAsyncOperation(undefined);
  }

  async removeFamily(id: string): Promise<void> {
    const index = this.families.findIndex(f => f.id === id);
    if (index !== -1) {
      this.families.splice(index, 1);
    }
    return simulateAsyncOperation(undefined);
  }
}
