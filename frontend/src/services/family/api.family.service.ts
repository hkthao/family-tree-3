import type { Family } from '@/types/family';
import type { IFamilyService } from './family.service.interface';
import axios from 'axios';

// Base URL for your API - configure this based on your environment
const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || '/api';

export class ApiFamilyService implements IFamilyService {
  private apiUrl = `${API_BASE_URL}/families`;

  async fetchFamilies(): Promise<Family[]> {
    const response = await axios.get<Family[]>(this.apiUrl);
    return response.data;
  }

  async getFamilyById(id: string): Promise<Family | undefined> {
    const response = await axios.get<Family>(`${this.apiUrl}/${id}`);
    return response.data;
  }

  async addFamily(newFamily: Omit<Family, 'id'>): Promise<Family> {
    const response = await axios.post<Family>(this.apiUrl, newFamily);
    return response.data;
  }

  async updateFamily(updatedFamily: Family): Promise<Family> {
    const response = await axios.put<Family>(`${this.apiUrl}/${updatedFamily.id}`, updatedFamily);
    return response.data;
  }

  async deleteFamily(id: string): Promise<void> {
    await axios.delete(`${this.apiUrl}/${id}`);
  }
}
