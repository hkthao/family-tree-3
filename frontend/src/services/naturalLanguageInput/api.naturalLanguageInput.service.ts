import { type ApiClientMethods, type ApiError } from '@/plugins/axios';
import type { GenerateDataRequest } from '@/types';
import { type Result } from '@/types/common';
import type { INaturalLanguageInputService } from './naturalLanguageInput.service.interface';
import type { Family } from '@/types/family';
import type { Member } from '@/types/family/member';
import type { Event } from '@/types/event';
import type { Relationship } from '@/types/relationship'; // Added Relationship

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;

export class ApiNaturalLanguageInputService implements INaturalLanguageInputService {
  constructor(private http: ApiClientMethods) {}

  private naturalLanguageInputApiUrl = `${API_BASE_URL}/NaturalLanguageInput`;
  private familyApiUrl = `${API_BASE_URL}/Family`;
  private membersApiUrl = `${API_BASE_URL}/Members`;
  private relationshipApiUrl = `${API_BASE_URL}/Relationship`; // Added relationshipApiUrl

  async generateFamilyData(prompt: string): Promise<Result<Family[], ApiError>> {
    const requestBody: GenerateDataRequest = { prompt };
    return this.http.post<Family[]>(`${this.familyApiUrl}/generate-family-data`, requestBody);
  }

  async generateMemberData(prompt: string): Promise<Result<Member[], ApiError>> {
    const requestBody: GenerateDataRequest = { prompt };
    return this.http.post<Member[]>(`${this.membersApiUrl}/generate-member-data`, requestBody);
  }

  async generateEventData(prompt: string): Promise<Result<Event[], ApiError>> {
    const requestBody: GenerateDataRequest = { prompt };
    return this.http.post<Event[]>(`${API_BASE_URL}/Events/generate-event-data`, requestBody);
  }

  async generateRelationshipData(prompt: string): Promise<Result<Relationship[], ApiError>> {
    const requestBody: GenerateDataRequest = { prompt };
    return this.http.post<Relationship[]>(`${this.relationshipApiUrl}/generate-relationship-data`, requestBody);
  }
}