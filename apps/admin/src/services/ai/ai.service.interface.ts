import type { Result, ApiError } from '@/types';

export interface IAiService {
  generateFamilyKb(familyId: string): Promise<Result<any, ApiError>>;
}