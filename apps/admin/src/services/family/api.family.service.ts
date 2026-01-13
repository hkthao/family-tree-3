import type { IFamilyService } from './family.service.interface';
import type { ApiClientMethods } from '@/plugins/axios';
import {
  type FamilyDto, // Renamed from Family
  type IFamilyAccess,
  type FamilyAddDto,
  type FamilyUpdateDto,
  type FamilyLimitConfiguration, // NEW
  type FamilyImportDto, // NEW
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





  async exportFamilyPdf(familyId: string, htmlContent: string): Promise<Result<Blob>> {
    return this.http.post<Blob>(`/family-data/${familyId}/export-pdf`, htmlContent, { headers: { 'Content-Type': 'text/html' }, responseType: 'blob' });
  }

  async getPrivacyConfiguration(familyId: string): Promise<Result<PrivacyConfiguration>> {
    const result = await this.http.get<PrivacyConfiguration>(`/family/${familyId}/privacy-configuration`);
    if (result.ok) {
      // Ensure all property arrays are initialized, even if backend returns null/undefined for some
      result.value.publicMemberProperties = result.value.publicMemberProperties || [];
      result.value.publicEventProperties = result.value.publicEventProperties || [];
      result.value.publicFamilyProperties = result.value.publicFamilyProperties || [];
      result.value.publicFamilyLocationProperties = result.value.publicFamilyLocationProperties || [];
      result.value.publicMemoryItemProperties = result.value.publicMemoryItemProperties || [];
      result.value.publicMemberFaceProperties = result.value.publicMemberFaceProperties || [];
      result.value.publicFoundFaceProperties = result.value.publicFoundFaceProperties || [];
    }
    return result;
  }

  async updatePrivacyConfiguration(familyId: string, settings: PrivacyConfiguration): Promise<Result<void>> {
    const payload = {
      familyId,
      publicMemberProperties: settings.publicMemberProperties,
      publicEventProperties: settings.publicEventProperties,
      publicFamilyProperties: settings.publicFamilyProperties,
      publicFamilyLocationProperties: settings.publicFamilyLocationProperties,
      publicMemoryItemProperties: settings.publicMemoryItemProperties,
      publicMemberFaceProperties: settings.publicMemberFaceProperties,
      publicFoundFaceProperties: settings.publicFoundFaceProperties,
    };
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

  async getFamilyLimitConfiguration(familyId: string): Promise<Result<FamilyLimitConfiguration>> {
    return this.http.get<FamilyLimitConfiguration>(`/family/${familyId}/limit-configuration`);
  }

  async updateFamilyLimitConfiguration(familyId: string, payload: { maxMembers: number; maxStorageMb: number; aiChatMonthlyLimit: number }): Promise<Result<void>> {
    return this.http.put<void>(`/family/${familyId}/limit-configuration`, payload);
  }

  async importFamilyData(familyData: FamilyImportDto, clearExistingData: boolean): Promise<Result<string>> {
    return this.http.post<string>(`/family/import?clearExistingData=${clearExistingData}`, familyData);
  }
}

