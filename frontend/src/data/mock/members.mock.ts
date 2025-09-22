import { faker } from '@faker-js/faker';

export const mockMembers = Array.from({ length: 50 }, () => {
  const gender = faker.helpers.arrayElement(['male', 'female', 'other']);
  const status = faker.helpers.arrayElement(['Alive', 'Deceased']);
  const dob = faker.date.past({ years: 100 });
  const dod = status === 'Deceased' ? faker.date.between({ from: dob, to: new Date() }) : undefined;

  const sex = gender === 'male' ? 'male' : (gender === 'female' ? 'female' : undefined);

  return {
    id: faker.string.uuid(),
    familyId: faker.string.uuid(), // Placeholder, will be linked in actual app
    fullName: faker.person.fullName({ sex }),
    givenName: faker.person.firstName(sex),
    nicknames: faker.helpers.arrayElements([faker.person.firstName(), faker.person.lastName()], { min: 0, max: 2 }),
    gender: gender,
    dob: dob.toISOString(),
    dod: dod ? dod.toISOString() : undefined,
    status: status,
    avatarUrl: faker.image.avatar(),
    contactEmail: faker.internet.email(),
    contactPhone: faker.phone.number(),
    generation: faker.number.int({ min: 1, max: 5 }),
    orderInFamily: faker.number.int({ min: 1, max: 10 }),
    description: faker.lorem.paragraph(),
    metadata: { hobbies: faker.lorem.words(3), occupation: faker.person.jobTitle() },
    createdAt: faker.date.past().toISOString(),
    updatedAt: faker.date.recent().toISOString(),
  };
});