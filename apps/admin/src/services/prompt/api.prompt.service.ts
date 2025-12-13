// apps/admin/src/services/prompt/api.prompt.service.ts

import type { ApiClientMethods } from '@/plugins/axios';
import type { IPromptService } from './prompt.service.interface';
import type { Prompt } from '@/types/prompt';
import { ApiCrudService } from '../common/api.crud.service';

export class ApiPromptService extends ApiCrudService<Prompt> implements IPromptService {
  constructor(protected http: ApiClientMethods) {
    super(http, '/prompts');
  }
}
