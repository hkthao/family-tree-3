// apps/admin/src/services/prompt/prompt.service.interface.ts

import type { Prompt } from '@/types/prompt';
import type { ICrudService } from '../common/crud.service.interface';
export interface IPromptService extends ICrudService<Prompt> {
  
}
