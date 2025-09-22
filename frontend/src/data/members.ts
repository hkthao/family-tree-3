import { ref, computed } from 'vue';
import type { Member, MemberFilter } from '@/types/member';
import { faker } from '@faker-js/faker';
import { useFamilies } from './families';
import type { Family } from '@/types/family';

const generateMockMembers = (count: number, families: Family[]): Member[] => {
  const members: Member[] = [];
  const genders = ['Male', 'Female', 'Other'];
  const familyIds = families.map(f => f.id);

  for (let i = 0; i < count; i++) {
    const gender = faker.helpers.arrayElement(genders) as Member['gender'];
    const firstName = faker.person.firstName(gender.toLowerCase() as 'male' | 'female');
    const lastName = faker.person.lastName();
    const fullName = `${firstName} ${lastName}`;
    const dateOfBirth = faker.date.past({ years: 80 });
    const dateOfDeath = faker.datatype.boolean() ? faker.date.soon({ refDate: dateOfBirth }) : undefined;

    members.push({
      id: faker.string.uuid(),
      fullName: fullName,
      nickname: faker.person.firstName(),
      dateOfBirth: dateOfBirth,
      dateOfDeath: dateOfDeath,
      gender: gender,
      placeOfBirth: faker.location.city() + ', ' + faker.location.country(),
      placeOfDeath: faker.datatype.boolean() ? faker.location.city() + ', ' + faker.location.country() : undefined,
      occupation: faker.person.jobTitle(),
      biography: faker.lorem.paragraph(),
      avatarUrl: faker.image.avatar(),
      familyId: faker.helpers.arrayElement(familyIds),
      fatherId: faker.datatype.boolean() ? faker.string.uuid() : undefined,
      motherId: faker.datatype.boolean() ? faker.string.uuid() : undefined,
      spouseId: faker.datatype.boolean() ? faker.string.uuid() : undefined,
    });
  }
  return members;
};

export const mockMembers = ref<Member[]>([]);

const { getFamilies } = useFamilies();
getFamilies('', 'All', 1, -1).then(data => {
  mockMembers.value = generateMockMembers(50, data.families);
});

export function useMembers() {
  const members = ref<Member[]>(mockMembers.value);

  const getMembers = (filter: MemberFilter = {}, page = 1, itemsPerPage = 10) => {
    let filtered = members.value;

    if (filter.fullName) {
      filtered = filtered.filter(member =>
        member.fullName.toLowerCase().includes(filter.fullName!.toLowerCase())
      );
    }
    if (filter.gender) {
      filtered = filtered.filter(member => member.gender === filter.gender);
    }
    if (filter.familyId) {
      filtered = filtered.filter(member => member.familyId === filter.familyId);
    }
    // Add more filters as needed

    const total = filtered.length;
    const start = (page - 1) * itemsPerPage;
    const end = start + itemsPerPage;
    const paginated = filtered.slice(start, end);

    return { members: paginated, total };
  };

  const getMemberById = (id: string) => {
    return mockMembers.value.find(member => member.id === id);
  };

  const addMember = (newMember: Omit<Member, 'id'>) => {
    const memberWithId = { ...newMember, id: faker.string.uuid() };
    members.value.push(memberWithId);
    return memberWithId;
  };

  const updateMember = (updatedMember: Member) => {
    const index = members.value.findIndex(member => member.id === updatedMember.id);
    if (index !== -1) {
      members.value[index] = updatedMember;
    }
    return updatedMember;
  };

  const deleteMember = (id: string) => {
    const initialLength = members.value.length;
    members.value = members.value.filter(member => member.id !== id);
    return members.value.length < initialLength; // true if deleted
  };

  return {
    members: computed(() => members.value),
    getMembers,
    getMemberById,
    addMember,
    updateMember,
    deleteMember,
  };
}
