import type { Member, Relationship } from '@/types';

declare global {
  interface Window {
    familyTreeData?: {
      familyId?: string;
      members?: Member[];
      relationships?: Relationship[];
    };
  }
}
