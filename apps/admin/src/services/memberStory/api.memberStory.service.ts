import type { IMemberStoryService } from './memberStory.service.interface';
import { type ApiClientMethods } from '@/plugins/axios';
import { type MemberStoryDto } from '@/types'; 
import { ApiCrudService } from '../common/api.crud.service';

export class ApiMemberStoryService extends ApiCrudService<MemberStoryDto>  implements IMemberStoryService {
  constructor(protected http: ApiClientMethods) {
    super(http, '/member-stories');
  }
}