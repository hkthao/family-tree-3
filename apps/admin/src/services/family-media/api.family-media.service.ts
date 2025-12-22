import type { ApiClientMethods } from '@/plugins/axios';
import type { IFamilyMediaService } from './family-media.service.interface';
import { type Result, ok, err } from '@/types';
import type { FamilyMedia, FamilyMediaFilter, Paginated, FamilyMediaAddFromUrlDto, ListOptions } from '@/types'; // Import from '@/types'

// ... other imports

export class ApiFamilyMediaService implements IFamilyMediaService {
  constructor(private api: ApiClientMethods) { }



  async search(
    listOptions: ListOptions,
    filters: FamilyMediaFilter,
  ): Promise<Result<Paginated<FamilyMedia>>> {
    const params: Record<string, any> = { ...filters };
    if (listOptions.page) params.page = listOptions.page;
    if (listOptions.itemsPerPage) params.itemsPerPage = listOptions.itemsPerPage;
    if (listOptions.sortBy && listOptions.sortBy.length > 0) {
      params.orderBy = listOptions.sortBy.map((s: { key: string; order: string }) => `${s.key} ${s.order}`).join(',');
    }

    return await this.api.get<Paginated<FamilyMedia>>(`/family-media/search`, { params });
  }

  async getById(id: string): Promise<Result<FamilyMedia>> {
    return await this.api.get<FamilyMedia>(`/family-media/${id}`);
  }

  async create(familyId: string, file: File, description?: string): Promise<Result<FamilyMedia>> {
    const formData = new FormData();
    formData.append('File', file);
    if (description) {
      formData.append('Description', description);
    }
    return await this.api.post<FamilyMedia>(`/family-media/${familyId}`, formData, {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
    });
  }

  async addFromUrl(familyId: string, payload: FamilyMediaAddFromUrlDto): Promise<Result<FamilyMedia>> {
    return await this.api.post<FamilyMedia>(`/family-media/${familyId}/from-url`, payload);
  }

  async delete(id: string): Promise<Result<boolean>> {
    return await this.api.delete(`/family-media/${id}`);
  }
}
