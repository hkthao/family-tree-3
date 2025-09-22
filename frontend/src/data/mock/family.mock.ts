import { faker } from '@faker-js/faker';
import type { Family } from '@/types/family';

export function generateMockFamily(id?: string): Family {
  return {
    id: id || faker.string.uuid(),
    name: faker.person.lastName() + ' Family',
    description: faker.lorem.sentence(),
    visibility: faker.helpers.arrayElement(['public', 'private']),
  };
}

export function generateMockFamilies(count: number): Family[] {
  const families: Family[] = [];
  for (let i = 0; i < count; i++) {
    families.push(generateMockFamily());
  }
  return families;
}
