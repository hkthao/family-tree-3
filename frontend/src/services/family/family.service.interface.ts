import type { Family } from '@/types/family';

export interface IFamilyService {
  fetchFamilies(): Promise<Family[]>;
  getFamilyById(id: string): Promise<Family | undefined>;
  addFamily(newFamily: Omit<Family, 'id'>): Promise<Family>;
  updateFamily(updatedFamily: Family): Promise<Family>;
  deleteFamily(id: string): Promise<void>;
}
