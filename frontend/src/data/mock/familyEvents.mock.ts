import { faker } from '@faker-js/faker';

export const mockFamilyEvents = Array.from({ length: 30 }, () => {
  const eventTypes = ['Birth', 'Marriage', 'Death', 'Migration', 'Other'];
  const startDate = faker.date.past({ years: 50 });
  const endDate = faker.date.soon({ refDate: startDate });

  return {
    id: faker.string.uuid(),
    familyId: faker.string.uuid(), // Placeholder
    name: faker.lorem.sentence({ min: 3, max: 7 }),
    type: faker.helpers.arrayElement(eventTypes),
    startDate: startDate.toISOString(),
    endDate: faker.helpers.arrayElement([undefined, endDate.toISOString()]),
    location: faker.location.city() + ', ' + faker.location.country(),
    description: faker.lorem.paragraph(),
    color: faker.color.rgb(),
    relatedMemberIds: Array.from({ length: faker.number.int({ min: 0, max: 3 }) }, () => faker.string.uuid()),
    createdAt: faker.date.past().toISOString(),
    updatedAt: faker.date.recent().toISOString(),
  };
});