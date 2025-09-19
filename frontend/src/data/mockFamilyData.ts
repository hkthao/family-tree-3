import { ref } from 'vue';

export interface FamilyMember {
  id: string;
  fullName: string;
  dateOfBirth: string;
  dateOfDeath?: string;
  placeOfBirth?: string;
  gender: 'Male' | 'Female' | 'Other';
  avatarUrl?: string;
  phone?: string;
  email?: string;
  generation?: number;
  biography?: string;
  metadata?: Record<string, unknown>;
  relationships: {
    type: 'parent' | 'child' | 'spouse' | 'sibling';
    memberId: string;
  }[];
}

export const mockFamilyMembers = ref<FamilyMember[]>([
  {
    id: '1',
    fullName: 'Nguyễn Văn A',
    dateOfBirth: '1950-01-15',
    gender: 'Male',
    relationships: [],
  },
  {
    id: '2',
    fullName: 'Trần Thị B',
    dateOfBirth: '1955-03-20',
    gender: 'Female',
    relationships: [{ type: 'spouse', memberId: '1' }],
  },
  {
    id: '3',
    fullName: 'Nguyễn Văn C',
    dateOfBirth: '1980-07-10',
    gender: 'Male',
    relationships: [
      { type: 'child', memberId: '1' },
      { type: 'child', memberId: '2' },
    ],
  },
  {
    id: '4',
    fullName: 'Lê Thị D',
    dateOfBirth: '1982-09-25',
    gender: 'Female',
    relationships: [{ type: 'spouse', memberId: '3' }],
  },
  {
    id: '5',
    fullName: 'Nguyễn Thị E',
    dateOfBirth: '2005-02-28',
    gender: 'Female',
    relationships: [
      { type: 'child', memberId: '3' },
      { type: 'child', memberId: '4' },
    ],
  },
  {
    id: '6',
    fullName: 'Phạm Văn F',
    dateOfBirth: '1978-11-01',
    gender: 'Male',
    relationships: [{ type: 'sibling', memberId: '3' }],
  },
]);
