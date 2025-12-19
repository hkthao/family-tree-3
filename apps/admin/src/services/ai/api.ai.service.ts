
import type { IAiService } from './ai.service.interface';
import type { ApiClientMethods } from '@/plugins/axios';

export class ApiAiService implements IAiService {
  constructor(private apiClient: ApiClientMethods) {}
}
