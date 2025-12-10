import type { IFamilyService } from './family.service.interface';
import type { ApiClientMethods } from '@/plugins/axios';
import {
  type Family,
  type IFamilyAccess,
  type FamilyExportDto,
  type GenerateFamilyDataCommand,
  type AnalyzedDataDto,
} from '@/types';
import type { Result } from '@/types';
import type { PrivacyConfiguration } from '@/stores/privacy-configuration.store';
import { ApiCrudService } from '../common/api.crud.service';

export class ApiFamilyService extends ApiCrudService<Family> implements IFamilyService {
  constructor(protected http: ApiClientMethods) {
    super(http, '/family');
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

  async generateFamilyData(command: GenerateFamilyDataCommand): Promise<Result<AnalyzedDataDto>> {
    return this.http.post<AnalyzedDataDto>(`/family/generate-data`, command);
  }
}
