import { ref, computed } from 'vue';
import type { Member, MemberFilter } from '@/types/member';

const mockMembers = ref<Member[]>([
  {
    id: '1',
    fullName: 'Nguyen Van A',
    nickname: 'A',
    dateOfBirth: '1950-01-15',
    dateOfDeath: '2020-03-10',
    gender: 'Male',
    placeOfBirth: 'Hanoi',
    occupation: 'Engineer',
    familyId: '1',
  },
  {
    id: '2',
    fullName: 'Tran Thi B',
    nickname: 'B',
    dateOfBirth: '1955-05-20',
    gender: 'Female',
    placeOfBirth: 'Hanoi',
    occupation: 'Teacher',
    familyId: '1',
  },
  {
    id: '3',
    fullName: 'Nguyen Van C',
    nickname: 'C',
    dateOfBirth: '1980-11-01',
    gender: 'Male',
    placeOfBirth: 'Hanoi',
    occupation: 'Doctor',
    familyId: '1',
    fatherId: '1',
    motherId: '2',
  },
  {
    id: '4',
    fullName: 'Nguyen Thi D',
    nickname: 'D',
    dateOfBirth: '1982-07-22',
    gender: 'Female',
    placeOfBirth: 'Hanoi',
    occupation: 'Artist',
    familyId: '1',
    fatherId: '1',
    motherId: '2',
  },
  {
    id: '5',
    fullName: 'Le Thi E',
    nickname: 'E',
    dateOfBirth: '1985-02-10',
    gender: 'Female',
    placeOfBirth: 'Ho Chi Minh City',
    occupation: 'Designer',
    familyId: '2',
  },
  {
    id: '6',
    fullName: 'Nguyen Van F',
    nickname: 'F',
    dateOfBirth: '2010-09-05',
    gender: 'Male',
    placeOfBirth: 'Hanoi',
    familyId: '1',
    fatherId: '3',
    motherId: '5',
  },
]);

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
    const memberWithId = { ...newMember, id: Date.now().toString() };
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
