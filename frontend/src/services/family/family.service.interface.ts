import type { Family, FamilyFilter, Result, Paginated } from "@/types";
import type { ApiError } from "@/plugins/axios";
import type { ICrudService } from "../common/crud.service.interface";

export interface IFamilyService extends ICrudService<Family> {
  loadItems(
    filter: FamilyFilter,
    page: number,
    itemsPerPage: number
  ): Promise<Result<Paginated<Family>, ApiError>>; // Keep searchFamilies
  getByIds(ids: string[]): Promise<Result<Family[], ApiError>>; // New method for fetching multiple families by IDs
}
