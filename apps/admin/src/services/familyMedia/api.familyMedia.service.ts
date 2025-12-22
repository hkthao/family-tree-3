import type { IFamilyMediaService } from './familyMedia.service.interface';
import type { ApiClientMethods } from '@/plugins/axios';
import type { Result } from '@/types/result';
import type { ApiError } from '@/types/apiError';
import type { Paginated, ListOptions } from '@/types/pagination';
import type { MediaItem } from '@/types/familyMedia';
import type { FamilyMediaFilter } from '@/types/familyMedia';

export class ApiFamilyMediaService implements IFamilyMediaService {
  constructor(private apiClient: ApiClientMethods) {}

  async search(
    listOptions: ListOptions,
    filters: FamilyMediaFilter,
  ): Promise<Result<Paginated<MediaItem>, ApiError>> {
    const result = await this.apiClient.get<Paginated<MediaItem>>('/family-media', {
      params: {
        page: listOptions.page,
        itemsPerPage: listOptions.itemsPerPage,
        sortBy: listOptions.sortBy?.map(s => `${s.key}:${s.order}`).join(','),
        searchQuery: filters.searchQuery,
        mediaType: filters.mediaType,
        familyId: filters.familyId,
      },
    });
    return result;
  }
}