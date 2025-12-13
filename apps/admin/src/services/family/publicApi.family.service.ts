import type { IPublicFamilyService } from './public.service.interface';
import { type ApiClientMethods } from '@/plugins/axios';
import { type Result, type Family, type ApiError } from '@/types';

export class PublicApiFamilyService implements IPublicFamilyService {
  constructor(private http: ApiClientMethods) {}

  async getPublicFamilyById(id: string): Promise<Result<Family, ApiError>> {
    return this.http.get<Family>(`/public/family/${id}`);
  }
}