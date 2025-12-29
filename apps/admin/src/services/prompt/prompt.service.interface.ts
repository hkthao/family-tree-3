// apps/admin/src/services/prompt/prompt.service.interface.ts

import type { Prompt, AddPromptDto, UpdatePromptDto } from '@/types/prompt';
import type { ICrudService } from '../common/crud.service.interface';
import type { ApiError, Result } from '@/types';
export interface IPromptService extends ICrudService<Prompt, AddPromptDto, UpdatePromptDto> {
  exportPrompts(): Promise<Result<Prompt[], ApiError>>;
  importPrompts(prompts: AddPromptDto[]): Promise<Result<Prompt[], ApiError>>;
}
