import { faker } from '@faker-js/faker';

export const mockFamilies = Array.from({ length: 10 }, (_, i) => ({
  id: faker.string.uuid(),
  name: faker.company.name() + ' Family',
  description: faker.lorem.paragraph(),
  address: faker.location.streetAddress(),
  avatarUrl: faker.image.urlLoremFlickr({ category: 'family' }),
  visibility: i % 2 === 0 ? 'Public' : 'Private',
  createdAt: faker.date.past().toISOString(),
  updatedAt: faker.date.recent().toISOString(),
}));
