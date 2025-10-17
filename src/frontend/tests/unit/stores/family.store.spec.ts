
import { useFamilyStore } from '@/stores/family.store';
import { defineCrudTests } from '../../shared/crudTests';
import { MockFamilyService } from '../../shared/mock.services';
import type { Family } from '@/types';
import { FamilyVisibility } from '@/types';
import { describe, it } from 'vitest';

const mockFamilyService = new MockFamilyService();

const entitySample: Family = {
  id: 'family-001',
  name: 'Test Family',
  description: 'A family for testing',
  visibility: FamilyVisibility.Public,
};

defineCrudTests(
  'Family Store',
  useFamilyStore,
  mockFamilyService,
  'family',
  entitySample,
);

describe('MemberStore extra cases', () => {
  it('should return adults only', () => {
    // const store = useMemberStore();
    // store.items = [
    //   { id: 'm1', name: 'John', age: 30 },
    //   { id: 'm2', name: 'Kid', age: 10 },
    // ];
    // expect(store.getAdults().length).toBe(1);
  });
});