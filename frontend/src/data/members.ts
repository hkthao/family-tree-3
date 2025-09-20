import { ref, computed } from 'vue';
import type { Member, MemberFilter } from '@/types/member';

const mockMembers = ref<Member[]>([
  {
    id: '1',
    fullName: 'Nguyen Van A',
    dateOfBirth: '1950-01-15',
    dateOfDeath: '2020-03-10',
    gender: 'Male',
    placeOfBirth: 'Hanoi',
    occupation: 'Engineer',
    parents: [],
    spouses: ['2'],
    children: ['3', '4'],
  },
  {
    id: '2',
    fullName: 'Tran Thi B',
    dateOfBirth: '1955-05-20',
    gender: 'Female',
    placeOfBirth: 'Hanoi',
    occupation: 'Teacher',
    parents: [],
    spouses: ['1'],
    children: ['3', '4'],
  },
  {
    id: '3',
    fullName: 'Nguyen Van C',
    dateOfBirth: '1980-11-01',
    gender: 'Male',
    placeOfBirth: 'Hanoi',
    occupation: 'Doctor',
    parents: ['1', '2'],
    spouses: ['5'],
    children: ['6'],
  },
  {
    id: '4',
    fullName: 'Nguyen Thi D',
    dateOfBirth: '1982-07-22',
    gender: 'Female',
    placeOfBirth: 'Hanoi',
    occupation: 'Artist',
    parents: ['1', '2'],
    spouses: [],
    children: [],
  },
  {
    id: '5',
    fullName: 'Le Thi E',
    dateOfBirth: '1985-02-10',
    gender: 'Female',
    placeOfBirth: 'Ho Chi Minh City',
    occupation: 'Designer',
    parents: [],
    spouses: ['3'],
    children: ['6'],
  },
  {
    id: '6',
    fullName: 'Nguyen Van F',
    dateOfBirth: '2010-09-05',
    gender: 'Male',
    placeOfBirth: 'Hanoi',
    parents: ['3', '5'],
    spouses: [],
    children: [],
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
    // Add more filters as needed

    const total = filtered.length;
    const start = (page - 1) * itemsPerPage;
    const end = start + itemsPerPage;
    const paginated = filtered.slice(start, end);

    return { members: paginated, total };
  };

  const getMemberById = (id: string) => {
    return members.value.find(member => member.id === id);
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
