import type { ApiClientMethods } from '@/plugins/axios';
import type { IFamilyMediaService } from './family-media.service.interface';
import { type Result } from '@/types'; // Change to normal import with ok, err
import type {  FamilyMedia, MediaLink, FamilyMediaFilter, Paginated } from '@/types'; // Import from '@/types'

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
    if (page) params.page = page;
    if (itemsPerPage) params.pageSize = itemsPerPage; // Backend uses pageSize
    if (sortBy && sortBy.length > 0) {
      params.orderBy = sortBy.map(s => `${s.key} ${s.order}`).join(',');
    }
    return await this.api.get<Paginated<FamilyMedia>>(`/family/${familyId}/media`, { params });
  }

  async getById(familyId: string, id: string): Promise<Result<FamilyMedia>> {
    return await this.api.get<FamilyMedia>(`/family/${familyId}/media/${id}`);
  }

  async create(familyId: string, file: File, description?: string): Promise<Result<string>> {
    const formData = new FormData();
    formData.append('File', file);
    if (description) {
      formData.append('Description', description);
    }
    formData.append('FamilyId', familyId); // Ensure FamilyId is part of the form data

    return await this.api.post<string>(`/family/${familyId}/media`, formData, {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
    });
  }

  async delete(familyId: string, id: string): Promise<Result<boolean>> {
    return await this.api.delete(`/family/${familyId}/media/${id}`);
  }

  async linkMediaToEntity(familyId: string, familyMediaId: string, refType: string, refId: string): Promise<Result<string>> {
    return await this.api.post<string>(`/family/${familyId}/media/${familyMediaId}/link`, { refType, refId });
  }

  async unlinkMediaFromEntity(familyId: string, familyMediaId: string, refType: string, refId: string): Promise<Result<boolean>> {
    return await this.api.delete(`/family/${familyId}/media/${familyMediaId}/link/${refType}/${refId}`);
  }

  async getMediaLinksByFamilyMediaId(familyId: string, familyMediaId: string): Promise<Result<MediaLink[]>> {
    return await this.api.get<MediaLink[]>(`/family/${familyId}/media/${familyMediaId}/links`);
  }

  async getMediaLinksByRefId(familyId: string, refType: string, refId: string): Promise<Result<MediaLink[]>> {
    return await this.api.get<MediaLink[]>(`/family/${familyId}/media/links/${refType}/${refId}`);
  }
}