import type { AxiosInstance } from 'axios';
import type { IFamilyMediaService } from './family-media.service.interface';
import type { Result } from '@/types/result';
import type { PaginatedList, FamilyMedia, MediaLink, FamilyMediaFilter } from '@/types'; // Import from '@/types'
import { ApiError } from '@/types/api.error'; // Import ApiError
import i18n from '@/plugins/i18n'; // Assuming i18n is available

export class ApiFamilyMediaService implements IFamilyMediaService {
  constructor(private api: AxiosInstance) {}

  async getFamilyMediaList(familyId: string, filters: FamilyMediaFilter): Promise<Result<PaginatedList<FamilyMedia>>> {
    try {
      const response = await this.api.get<PaginatedList<FamilyMedia>>(`/family/${familyId}/media`, { params: filters });
      return Result.success(response.data);
    } catch (error: any) {
      return Result.failure(ApiError.fromAxiosError(error, i18n.global.t('familyMedia.errors.loadList')));
    }
  }

  async getFamilyMediaById(familyId: string, id: string): Promise<Result<FamilyMedia>> {
    try {
      const response = await this.api.get<FamilyMedia>(`/family/${familyId}/media/${id}`);
      return Result.success(response.data);
    } catch (error: any) {
      return Result.failure(ApiError.fromAxiosError(error, i18n.global.t('familyMedia.errors.loadDetail')));
    }
  }

  async createFamilyMedia(familyId: string, file: File, description?: string): Promise<Result<string>> {
    try {
      const formData = new FormData();
      formData.append('File', file);
      if (description) {
        formData.append('Description', description);
      }
      formData.append('FamilyId', familyId); // Ensure FamilyId is part of the form data

      const response = await this.api.post<string>(`/family/${familyId}/media`, formData, {
        headers: {
          'Content-Type': 'multipart/form-data',
        },
      });
      return Result.success(response.data);
    } catch (error: any) {
      return Result.failure(ApiError.fromAxiosError(error, i18n.global.t('familyMedia.errors.create')));
    }
  }

  async deleteFamilyMedia(familyId: string, id: string): Promise<Result<boolean>> {
    try {
      await this.api.delete(`/family/${familyId}/media/${id}`);
      return Result.success(true);
    } catch (error: any) {
      return Result.failure(ApiError.fromAxiosError(error, i18n.global.t('familyMedia.errors.delete')));
    }
  }

  async linkMediaToEntity(familyId: string, familyMediaId: string, refType: string, refId: string): Promise<Result<string>> {
    try {
      const response = await this.api.post<string>(`/family/${familyId}/media/${familyMediaId}/link`, { refType, refId });
      return Result.success(response.data);
    } catch (error: any) {
      return Result.failure(ApiError.fromAxiosError(error, i18n.global.t('familyMedia.errors.link')));
    }
  }

  async unlinkMediaFromEntity(familyId: string, familyMediaId: string, refType: string, refId: string): Promise<Result<boolean>> {
    try {
      await this.api.delete(`/family/${familyId}/media/${familyMediaId}/link/${refType}/${refId}`);
      return Result.success(true);
    } catch (error: any) {
      return Result.failure(ApiError.fromAxiosError(error, i18n.global.t('familyMedia.errors.unlink')));
    }
  }

  async getMediaLinksByFamilyMediaId(familyId: string, familyMediaId: string): Promise<Result<MediaLink[]>> {
    try {
      const response = await this.api.get<MediaLink[]>(`/family/${familyId}/media/${familyMediaId}/links`);
      return Result.success(response.data);
    } catch (error: any) {
      return Result.failure(ApiError.fromAxiosError(error, i18n.global.t('familyMedia.errors.loadLinks')));
    }
  }

  async getMediaLinksByRefId(familyId: string, refType: string, refId: string): Promise<Result<MediaLink[]>> {
    try {
      const response = await this.api.get<MediaLink[]>(`/family/${familyId}/media/links/${refType}/${refId}`);
      return Result.success(response.data);
    } catch (error: any) {
      return Result.failure(ApiError.fromAxiosError(error, i18n.global.t('familyMedia.errors.loadLinks')));
    }
  }
}
