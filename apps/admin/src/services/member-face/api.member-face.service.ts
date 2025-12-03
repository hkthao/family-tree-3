import type { MemberFace, MemberFaceFilter, Paginated, Result, FaceDetectionRessult } from '@/types';
import type { IMemberFaceService } from './member-face.service.interface';
import { type ApiClientMethods, type ApiError } from '@/plugins/axios';
import { ok } from '@/types'; 

export class ApiMemberFaceService implements IMemberFaceService {
  constructor(private http: ApiClientMethods) {}

  async getById(id: string): Promise<Result<MemberFace | undefined, ApiError>> {
    const result = await this.http.get<MemberFace>(`/memberfaces/${id}`);
    if (result.ok) {
        return ok(result.value || undefined);
    }
    return result; 
  }

  async add(newItem: Omit<MemberFace, 'id'>): Promise<Result<MemberFace, ApiError>> {
    return await this.http.post<MemberFace>(`/memberfaces`, newItem);
  }

  async update(updatedItem: MemberFace): Promise<Result<MemberFace, ApiError>> {
    return await this.http.put<MemberFace>(`/memberfaces/${updatedItem.id}`, updatedItem);
  }

  async delete(id: string): Promise<Result<void, ApiError>> {
    return await this.http.delete<void>(`/memberfaces/${id}`);
  }

  async loadItems(
    filters: MemberFaceFilter,
    page: number,
    itemsPerPage: number,
    sortBy?: string,
    sortOrder?: 'asc' | 'desc',
  ): Promise<Result<Paginated<MemberFace>, ApiError>> {
    const params = new URLSearchParams();
    if (filters.memberId) params.append('memberId', filters.memberId);
    if (filters.familyId) params.append('familyId', filters.familyId);
    if (filters.searchQuery) params.append('searchQuery', filters.searchQuery);
    if (filters.emotion) params.append('emotion', filters.emotion);
    if (sortBy) params.append('sortBy', sortBy);
    if (sortOrder) params.append('sortOrder', sortOrder);

    params.append('page', page.toString());
    params.append('itemsPerPage', itemsPerPage.toString());

    return await this.http.get<Paginated<MemberFace>>(
      `/memberfaces?${params.toString()}`,
    );
  }

  async getByIds(ids: string[]): Promise<Result<MemberFace[], ApiError>> {
    const params = new URLSearchParams();
    params.append('ids', ids.join(','));
    return await this.http.get<MemberFace[]>(
      `/memberfaces/by-ids?${params.toString()}`,
    );
  }

  async detect(
    file: File,
    familyId: string,
    resizeImageForAnalysis?: boolean,
    returnCrop?: boolean,
  ): Promise<Result<FaceDetectionRessult, ApiError>> {
    const formData = new FormData();
    formData.append('file', file);

    const params = new URLSearchParams();
    params.append('familyId', familyId);
    if (resizeImageForAnalysis !== undefined) {
      params.append('resizeImageForAnalysis', resizeImageForAnalysis.toString());
    }
    if (returnCrop !== undefined) {
      params.append('returnCrop', returnCrop.toString());
    }

    return await this.http.post<FaceDetectionRessult>(`/memberfaces/detect?${params.toString()}`, formData, {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
    });
  }
}
