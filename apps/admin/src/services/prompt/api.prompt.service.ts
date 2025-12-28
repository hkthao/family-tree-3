// apps/admin/src/services/prompt/api.prompt.service.ts

import type { ApiClientMethods } from '@/plugins/axios';
import type { IPromptService } from './prompt.service.interface';
import type { Prompt, AddPromptDto, UpdatePromptDto } from '@/types/prompt';
import { ApiCrudService } from '../common/api.crud.service';
import type { ApiError, Result } from '@/types';

export class ApiPromptService extends ApiCrudService<Prompt, AddPromptDto, UpdatePromptDto> implements IPromptService {
  constructor(protected http: ApiClientMethods) {
    super(http, '/prompts');
  }

  async exportPrompts(): Promise<Result<Prompt[], ApiError>> {
    return this.http.get<Prompt[]>('/prompts/export');
  }

  async importPrompts(prompts: AddPromptDto[]): Promise<Result<Prompt[], ApiError>> {
    return this.http.post<Prompt[]>('/prompts/import', prompts);
  }
}
