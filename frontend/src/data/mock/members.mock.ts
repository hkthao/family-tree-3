import type { Member } from '../../types/member';
import { faker } from '@faker-js/faker'; // Added comment to force re-transpilation

export function generateMembersMock(): Member[] {
  const membersMock: Member[] = [];

  const genders = ['Male', 'Female', 'Other'];

  for (let i = 1; i <= 100; i++) { // Generate 100 members
    const gender = faker.helpers.arrayElement(genders);
    const firstName = faker.person.firstName(gender === 'Male' ? 'male' : (gender === 'Female' ? 'female' : undefined));
    const lastName = faker.person.lastName();

    membersMock.push({
      id: i.toString(),
      fullName: `${firstName} ${lastName}`,
      nickname: faker.internet.username({ firstName, lastName }),
      dateOfBirth: faker.date.past({ years: 80 }),
      dateOfDeath: faker.datatype.boolean(0.2) ? faker.date.recent({ days: 365 }) : undefined, // 20% chance of being deceased
      gender: gender as 'Male' | 'Female' | 'Other',
      placeOfBirth: faker.location.city(),
      placeOfDeath: faker.datatype.boolean(0.1) ? faker.location.city() : undefined,
      occupation: faker.person.jobTitle(),
      biography: faker.lorem.paragraph(),
      avatarUrl: faker.image.avatar(),
      familyId: faker.helpers.arrayElement(Array.from({ length: 55 }, (_, k) => (k + 1).toString())),
      fatherId: faker.datatype.boolean(0.5) ? faker.helpers.arrayElement(membersMock.filter(m => m.gender === 'Male').map(m => m.id)) : undefined,
      motherId: faker.datatype.boolean(0.5) ? faker.helpers.arrayElement(membersMock.filter(m => m.gender === 'Female').map(m => m.id)) : undefined,
      spouseId: faker.datatype.boolean(0.3) ? faker.helpers.arrayElement(membersMock.filter(m => m.gender !== gender).map(m => m.id)) : undefined,
    });
  }
  return membersMock;
}
