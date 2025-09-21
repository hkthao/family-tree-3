import type { Family } from '../../types/family';
import { faker } from '@faker-js/faker';

export const familiesMock: Family[] = [];

for (let i = 1; i <= 50; i++) {
  familiesMock.push({
    id: i.toString(),
    name: `Family ${i < 10 ? '0' + i : i} Name`,
    description: faker.lorem.paragraph(),
    address: faker.location.streetAddress(true),
    avatarUrl: faker.image.avatar(),
    visibility: i % 2 === 0 ? 'Public' : 'Private', // Alternating visibility for variety
  });
}

// Add some specific names for search testing
familiesMock.push({
  id: '51',
  name: 'The Alpha Family',
  description: faker.lorem.paragraph(),
  address: faker.location.streetAddress(true),
  avatarUrl: faker.image.avatar(),
  visibility: 'Public',
});
familiesMock.push({
  id: '52',
  name: 'The Beta Family',
  description: faker.lorem.paragraph(),
  address: faker.location.streetAddress(true),
  avatarUrl: faker.image.avatar(),
  visibility: 'Private',
});
familiesMock.push({
  id: '53',
  name: 'The Gamma Family',
  description: faker.lorem.paragraph(),
  address: faker.location.streetAddress(true),
  avatarUrl: faker.image.avatar(),
  visibility: 'Public',
});
familiesMock.push({
  id: '54',
  name: 'The Delta Family',
  description: faker.lorem.paragraph(),
  address: faker.location.streetAddress(true),
  avatarUrl: faker.image.avatar(),
  visibility: 'Private',
});
familiesMock.push({
  id: '55',
  name: 'The Epsilon Family',
  description: faker.lorem.paragraph(),
  address: faker.location.streetAddress(true),
  avatarUrl: faker.image.avatar(),
  visibility: 'Public',
});
