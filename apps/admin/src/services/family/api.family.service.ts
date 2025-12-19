import type { IFamilyService } from './family.service.interface';
import type { ApiClientMethods } from '@/plugins/axios';
import {
  type Family,
  type IFamilyAccess,
  type FamilyExportDto,
  type FamilyAddDto, // NEW
  type FamilyUpdateDto, // NEW
} from '@/types';
import type { Result } from '@/types';
import type { PrivacyConfiguration } from '@/types/privacyConfiguration';
import { ApiCrudService } from '../common/api.crud.service';

export class ApiFamilyService extends ApiCrudService<Family> implements IFamilyService {
  constructor(protected http: ApiClientMethods) {
    super(http, '/family');
  }

  async add(newItem: FamilyAddDto): Promise<Result<Family>> {
    return await this.http.post<Family>(this.baseUrl, newItem);
  }

  async update(updatedItem: FamilyUpdateDto): Promise<Result<Family>> {
    const { id } = updatedItem; // Extract id and deletedUserIds
    // The backend API is expected to handle deletedManagerIds and deletedViewerIds as part of the payload or query params
    // For now, we'll include it in the payload. Backend will decide how to process.
    return await this.http.put<Family>(
      `${this.baseUrl}/${id}`,
      updatedItem, // Include deletedManagerIds and deletedViewerIds in the payload
    );
  }

  async addItems(
    newItems: Omit<Family, 'id'>[],
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
}
