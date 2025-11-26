import type { Family, FamilyFilter, Result, Paginated } from "@/types";
import type { ApiError } from "@/plugins/axios";
import type { ICrudService } from "../common/crud.service.interface";
import type { FamilyExportDto } from '@/types/family'; // NEW IMPORT

export interface IFamilyService extends ICrudService<Family> {
  loadItems(
    filter: FamilyFilter,
    page: number,
    itemsPerPage: number
  ): Promise<Result<Paginated<Family>, ApiError>>; // Keep searchFamilies
  getByIds(ids: string[]): Promise<Result<Family[], ApiError>>; // New method for fetching multiple families by IDs
  addItems(newItems: Omit<Family, 'id'>[]): Promise<Result<string[], ApiError>>; // New method for bulk adding families
  getByIdWithDetails(id: string): Promise<Result<Family, ApiError>>; // New method to get family with full details
  exportFamilyData(familyId: string): Promise<Result<FamilyExportDto, ApiError>>; // NEW METHOD
  importFamilyData(familyId: string, familyData: FamilyExportDto, clearExistingData: boolean): Promise<Result<string, ApiError>>; // NEW METHOD
  exportFamilyPdf(familyId: string, htmlContent: string): Promise<Result<Blob, ApiError>>; // NEW METHOD
}
