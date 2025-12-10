import {
  type FamilyDict,
  type FamilyDictFilter,
  type FamilyDictImport,
  type PaginatedList, 
  type ListOptions, 
} from '@/types';
import { type IFamilyDictService } from './family-dict.service.interface';
import type { ApiClientMethods } from '@/plugins/axios';
import type { Result } from '@/types'; 
import { ApiCrudService } from '../common/api.crud.service'; 

export class ApiFamilyDictService extends ApiCrudService<FamilyDict> implements IFamilyDictService {
  constructor(protected http: ApiClientMethods) {
    super(http, '/family-dict'); 
  }
  
  async loadItems(
    filters: FamilyDictFilter,
    page: number = 1,
    itemsPerPage: number = 10,
  ): Promise<Result<PaginatedList<FamilyDict>>> { 
    const options: ListOptions = {
      page,
      itemsPerPage,
      sortBy: filters.sortBy && filters.sortOrder ? [{ key: filters.sortBy, order: filters.sortOrder }] : undefined,
    };
    
    const searchFilters: FamilyDictFilter & { [key: string]: any } = { ...filters };
    
    delete searchFilters.sortBy;
    delete searchFilters.sortOrder;

    return this.search(options, searchFilters);
  }

  async importItems(data: FamilyDictImport): Promise<Result<string[]>> { 
    
    return this.http.post<string[]>(`/family-dict/import`, data);
  }

  
}