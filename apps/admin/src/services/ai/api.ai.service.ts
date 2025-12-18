
import type { IAiService } from './ai.service.interface';
import type { ApiClientMethods } from '@/plugins/axios';
const AI_BASE_URL = '/ai'; 
export class ApiAiService implements IAiService {
  constructor(private apiClient: ApiClientMethods) {}
}
