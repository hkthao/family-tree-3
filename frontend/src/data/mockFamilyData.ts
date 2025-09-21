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

const member1Id = 'a1b2c3d4-e5f6-7890-1234-567890abcdef';
const member2Id = 'b2c3d4e5-f6a7-8901-2345-67890abcdef1';
const member3Id = 'c3d4e5f6-a7b8-9012-3456-7890abcdef12';
const member4Id = 'd4e5f6a7-b8c9-0123-4567-890abcdef123';
const member5Id = 'e5f6a7b8-c9d0-1234-5678-90abcdef1234';
const member6Id = 'f6a7b8c9-d0e1-2345-6789-0abcdef12345';

export const mockFamilyMembers = ref<FamilyMember[]>([
  {
    id: member1Id,
    fullName: 'Nguyễn Văn A',
    dateOfBirth: '1950-01-15',
    gender: 'Male',
    relationships: [],
  },
  {
    id: member2Id,
    fullName: 'Trần Thị B',
    dateOfBirth: '1955-03-20',
    gender: 'Female',
    relationships: [{ type: 'spouse', memberId: member1Id }],
  },
  {
    id: member3Id,
    fullName: 'Nguyễn Văn C',
    dateOfBirth: '1980-07-10',
    gender: 'Male',
    relationships: [
      { type: 'child', memberId: member1Id },
      { type: 'child', memberId: member2Id },
    ],
  },
  {
    id: member4Id,
    fullName: 'Lê Thị D',
    dateOfBirth: '1982-09-25',
    gender: 'Female',
    relationships: [{ type: 'spouse', memberId: member3Id }],
  },
  {
    id: member5Id,
    fullName: 'Nguyễn Thị E',
    dateOfBirth: '2005-02-28',
    gender: 'Female',
    relationships: [
      { type: 'child', memberId: member3Id },
      { type: 'child', memberId: member4Id },
    ],
  },
  {
    id: member6Id,
    fullName: 'Phạm Văn F',
    dateOfBirth: '1978-11-01',
    gender: 'Male',
    relationships: [{ type: 'sibling', memberId: member3Id }],
  },
]);