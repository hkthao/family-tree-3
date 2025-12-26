import type { IFamilyService } from './family.service.interface';
import type { ApiClientMethods } from '@/plugins/axios';
import {
  type FamilyDto, // Renamed from Family
  type IFamilyAccess,
  type FamilyExportDto,
  type FamilyAddDto,
  type FamilyUpdateDto,
} from '@/types';
import type { Result } from '@/types';
import type { PrivacyConfiguration } from '@/types/privacyConfiguration';
import { ApiCrudService } from '../common/api.crud.service';
import type { ListOptions, FilterOptions, Paginated } from '@/types'; // NEW

export class ApiFamilyService extends ApiCrudService<FamilyDto, FamilyAddDto, FamilyUpdateDto> implements IFamilyService {
  constructor(protected http: ApiClientMethods) {
    super(http, '/family');
  }

  async addItems(
    newItems: FamilyAddDto[],
  ): Promise<Result<string[]>> {

    return this.http.post<string[]>(`/family/bulk-create`, {
      families: newItems,
    });
  }

  async getUserFamilyAccess(): Promise<Result<IFamilyAccess[]>> {
    return this.http.get<IFamilyAccess[]>(`/family/my-access`);
  }

  async exportFamilyData(familyId: string): Promise<Result<FamilyExportDto>> {
    return this.http.get<FamilyExportDto>(`/family-data/${familyId}/export`);
  }

  async importFamilyData(familyId: string, familyData: FamilyExportDto, clearExistingData: boolean): Promise<Result<string>> {
    const queryParams = clearExistingData === false ? '?clearExistingData=false' : '';
    return this.http.post<string>(`/family-data/import/${familyId}${queryParams}`, familyData);
  }

  async exportFamilyPdf(familyId: string, htmlContent: string): Promise<Result<Blob>> {
    return this.http.post<Blob>(`/family-data/${familyId}/export-pdf`, htmlContent, { headers: { 'Content-Type': 'text/html' }, responseType: 'blob' });
  }

  async getPrivacyConfiguration(familyId: string): Promise<Result<PrivacyConfiguration>> {
    const result = await this.http.get<PrivacyConfiguration>(`/family/${familyId}/privacy-configuration`);
    if (result.ok) {
      result.value.publicMemberProperties = result.value.publicMemberProperties || [];
    }
    return result;
  }

  async updatePrivacyConfiguration(familyId: string, publicMemberProperties: string[]): Promise<Result<void>> {
    const payload = { familyId, publicMemberProperties };
    return this.http.put<void>(`/family/${familyId}/privacy-configuration`, payload);
  }

  async searchPublic(listOptions: ListOptions, filterOptions: FilterOptions): Promise<Result<Paginated<FamilyDto>>> {
    const params = {
      page: listOptions.page,
      itemsPerPage: listOptions.itemsPerPage,
      sortBy: listOptions.sortBy?.map(s => `${s.key}:${s.order}`).join(','),
      searchQuery: filterOptions.searchQuery,
    };
    // Clean up undefined/null values from params
    const cleanParams = Object.fromEntries(Object.entries(params).filter(([, v]) => v !== undefined && v !== null));

    return this.http.get<Paginated<FamilyDto>>(`/family/public-search`, { params: cleanParams });
  }
}

