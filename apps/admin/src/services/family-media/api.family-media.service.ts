import type { ApiClientMethods } from '@/plugins/axios';
import type { IFamilyMediaService } from './family-media.service.interface';
import { type Result } from '@/types'; // Change to normal import with ok, err
import type {  FamilyMedia, FamilyMediaFilter, Paginated } from '@/types'; // Import from '@/types'

export class ApiFamilyMediaService implements IFamilyMediaService {
  constructor(private api: ApiClientMethods) { }

  async search(
    familyId: string,
    filters: FamilyMediaFilter,
    page?: number,
    itemsPerPage?: number,
    sortBy?: { key: string; order: string }[],
  ): Promise<Result<Paginated<FamilyMedia>>> {
    const params: Record<string, any> = { ...filters };
    if (familyId) params.familyId = familyId; // Add familyId to params
    if (page) params.page = page;
    if (itemsPerPage) params.pageSize = itemsPerPage; // Backend uses pageSize
    if (sortBy && sortBy.length > 0) {
      params.orderBy = sortBy.map(s => `${s.key} ${s.order}`).join(',');
    }
    return await this.api.get<Paginated<FamilyMedia>>(`/family-media/search`, { params });
  }

  async getById(id: string): Promise<Result<FamilyMedia>> {
    return await this.api.get<FamilyMedia>(`/family-media/${id}`);
  }

  async create(familyId: string, file: File, description?: string): Promise<Result<string>> {
    const formData = new FormData();
    formData.append('File', file);
    if (description) {
      formData.append('Description', description);
    }
    return await this.api.post<string>(`/family-media/${familyId}`, formData, {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
    });
  }

  async delete(id: string): Promise<Result<boolean>> {
    return await this.api.delete(`/family-media/${id}`);
  }
}