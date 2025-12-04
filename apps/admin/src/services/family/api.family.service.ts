import type { IFamilyService } from './family.service.interface';
import { type ApiClientMethods, type ApiError } from '@/plugins/axios';
import {
  type Result,
  type Family,
  type FamilyFilter,
  type Paginated,
  type IFamilyAccess,
  type FamilyExportDto,
  type GenerateFamilyDataCommand,
  type AnalyzedDataDto
} from '@/types';
import type { PrivacyConfiguration } from '@/stores/privacy-configuration.store';

export class ApiFamilyService implements IFamilyService {
  constructor(private http: ApiClientMethods) { }
  async getById(id: string): Promise<Result<Family, ApiError>> {
    return this.http.get<Family>(`/family/${id}`);
  }
  async add(newItem: Omit<Family, 'id'>): Promise<Result<Family, ApiError>> {
    return this.http.post<Family>(`/family`, newItem);
  }
  async addItems(
    newItems: Omit<Family, 'id'>[],
  ): Promise<Result<string[], ApiError>> {
    return this.http.post<string[]>(`/family/bulk-create`, {
      families: newItems,
    });
  }
  async update(updatedItem: Family): Promise<Result<Family, ApiError>> {
    return this.http.put<Family>(
      `/family/${updatedItem.id}`,
      updatedItem,
    );
  }
  async delete(id: string): Promise<Result<void, ApiError>> {
    return this.http.delete<void>(`/family/${id}`);
  }
  async loadItems(
    filter: FamilyFilter,
    page: number,
    itemsPerPage: number,
  ): Promise<Result<Paginated<Family>, ApiError>> {
    const params = new URLSearchParams();
    if (filter.searchQuery) params.append('searchQuery', filter.searchQuery);
    if (filter.familyId) params.append('familyId', filter.familyId);
    if (filter.visibility) params.append('visibility', filter.visibility);
    params.append('page', page.toString());
    params.append('itemsPerPage', itemsPerPage.toString());
    if (filter.sortBy) params.append('sortBy', filter.sortBy);
    if (filter.sortOrder) params.append('sortOrder', filter.sortOrder);
    return this.http.get<Paginated<Family>>(
      `/family/search?${params.toString()}`,
    );
  }
  async getByIds(ids: string[]): Promise<Result<Family[], ApiError>> {
    const params = new URLSearchParams();
    params.append('ids', ids.join(','));
    return this.http.get<Family[]>(
      `/family/by-ids?${params.toString()}`,
    );
  }
  async getUserFamilyAccess(): Promise<Result<IFamilyAccess[], ApiError>> {
    return this.http.get<IFamilyAccess[]>(`/family/my-access`);
  }
  async exportFamilyData(familyId: string): Promise<Result<FamilyExportDto, ApiError>> {
    return this.http.get<FamilyExportDto>(`/family-data/${familyId}/export`);
  }
  async importFamilyData(familyId: string, familyData: FamilyExportDto, clearExistingData: boolean): Promise<Result<string, ApiError>> {
    const queryParams = clearExistingData === false ? '?clearExistingData=false' : '';
    return this.http.post<string>(`/family-data/import/${familyId}${queryParams}`, familyData);
  }
  async exportFamilyPdf(familyId: string, htmlContent: string): Promise<Result<Blob, ApiError>> {
    return this.http.post<Blob>(`/family-data/${familyId}/export-pdf`, htmlContent, { headers: { 'Content-Type': 'text/html' }, responseType: 'blob' });
  }
  async getPrivacyConfiguration(familyId: string): Promise<Result<PrivacyConfiguration, ApiError>> {
    const result = await this.http.get<PrivacyConfiguration>(`/PrivacyConfiguration/${familyId}`);
    if (result.ok) {
      result.value.publicMemberProperties = result.value.publicMemberProperties || [];
    }
    return result;
  }
  async updatePrivacyConfiguration(familyId: string, publicMemberProperties: string[]): Promise<Result<void, ApiError>> {
    const payload = { familyId, publicMemberProperties };
    return this.http.put<void>(`/PrivacyConfiguration/${familyId}`, payload);
  }
  async generateFamilyData(command: GenerateFamilyDataCommand): Promise<Result<AnalyzedDataDto, ApiError>> {
    return this.http.post<AnalyzedDataDto>(`/family/generate-data`, command);
  }
}
