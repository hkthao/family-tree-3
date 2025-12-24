import {
  type FamilyDict,
  type FamilyDictImport,
  type AddFamilyDictDto,
  type UpdateFamilyDictDto,
} from '@/types';
import { type IFamilyDictService } from './family-dict.service.interface';
import type { ApiClientMethods } from '@/plugins/axios';
import type { Result } from '@/types';
import { ApiCrudService } from '../common/api.crud.service';

export class ApiFamilyDictService extends ApiCrudService<FamilyDict, AddFamilyDictDto, UpdateFamilyDictDto> implements IFamilyDictService {  constructor(protected http: ApiClientMethods) {
    super(http, '/family-dict'); 
  }
  async importItems(data: FamilyDictImport): Promise<Result<string[]>> { 
    return this.http.post<string[]>(`/family-dict/import`, data);
  }
}