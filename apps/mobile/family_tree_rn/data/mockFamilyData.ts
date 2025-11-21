export interface FamilyDetail {
  id: string;
  name: string;
  code: string;
  description?: string;
  avatarUrl?: string;
  totalMembers: number;
  totalGenerations: number;
  visibility: 'Public' | 'Private';
  createdAt: string;
  lastUpdatedAt: string;
  createdBy: string;
  address?: string;
  manager: string[];
  viewers: string[];
}

export interface FamilyMember {
  id: string;
  name: string;
  avatarUrl?: string;
  relationship: string; // e.g., "Father", "Mother", "Son", "Daughter"
  gender: 'Male' | 'Female' | 'Other';
  isRootMember: boolean;
  ordinalNumber?: number;
  birthDeathYears?: string;
  occupation?: string;
  father?: string;
  mother?: string;
  wife?: string;
  husband?: string;
}

export interface FamilyEvent {
  id: string;
  name: string;
  date: string; // ISO date string
  description?: string;
}

const mockFamilies: FamilyDetail[] = [
  {
    id: '1',
    name: 'Gia đình Nguyễn',
    code: 'GDN001',
    description: 'Gia đình lớn ở Hà Nội với nhiều thế hệ và truyền thống lâu đời.',
    avatarUrl: 'https://picsum.photos/seed/family1/100/100',
    totalMembers: 50,
    totalGenerations: 5,
    visibility: 'Public',
    createdAt: '2023-01-15T10:00:00Z',
    lastUpdatedAt: '2024-05-20T14:30:00Z',
    createdBy: 'Nguyễn Văn A',
    address: '123 Đường Láng, Đống Đa, Hà Nội',
    manager: ['Nguyễn Văn A'],
    viewers: ['Nguyễn Thị B', 'Trần Văn C'],
  },
  {
    id: '2',
    name: 'Họ Trần',
    code: 'HTC002',
    description: 'Họ Trần ở Huế, nổi tiếng với các di tích lịch sử và văn hóa.',
    avatarUrl: 'https://picsum.photos/seed/family2/100/100',
    totalMembers: 120,
    totalGenerations: 8,
    visibility: 'Public',
    createdAt: '2022-03-01T08:00:00Z',
    lastUpdatedAt: '2024-06-10T11:00:00Z',
    createdBy: 'Trần Thị B',
    address: '456 Đường Hùng Vương, Thành phố Huế',
    manager: ['Trần Thị B', 'Trần Văn X'],
    viewers: ['Lê Văn D', 'Phạm Thị E', 'Hoàng Văn F'],
  },
  {
    id: '3',
    name: 'Gia đình Lê',
    code: 'GDL003',
    description: 'Gia đình nhỏ ở Sài Gòn, hiện đại và năng động.',
    avatarUrl: 'https://picsum.photos/seed/family3/100/100',
    totalMembers: 15,
    totalGenerations: 3,
    visibility: 'Private',
    createdAt: '2023-07-01T16:00:00Z',
    lastUpdatedAt: '2024-01-05T09:00:00Z',
    createdBy: 'Lê Văn C',
    address: '789 Đường Cách Mạng Tháng Tám, Quận 3, TP.HCM',
    manager: ['Lê Văn C'],
    viewers: ['Nguyễn Thị G'],
  },
];

export const fetchFamilyDetails = async (familyId: string): Promise<FamilyDetail | null> => {
  await new Promise((resolve) => setTimeout(resolve, 500)); // Simulate API delay
  return mockFamilies.find((family) => family.id === familyId) || null;
};

interface MemberFilter {
  gender?: 'Male' | 'Female' | 'Other';
  isRootMember?: boolean;
}

const mockMembers: FamilyMember[] = [
  { id: 'm1', name: 'Nguyễn Văn A', avatarUrl: 'https://picsum.photos/seed/member1/50/50', relationship: 'Trưởng tộc', gender: 'Male', isRootMember: true, ordinalNumber: 1, birthDeathYears: '1900-1980', occupation: 'Nông dân', father: 'Nguyễn Văn Cha', mother: 'Trần Thị Mẹ', wife: 'Trần Thị X' },
  { id: 'm2', name: 'Trần Thị X', avatarUrl: 'https://picsum.photos/seed/member2/50/50', relationship: 'Vợ', gender: 'Female', isRootMember: false, birthDeathYears: '1905-1985', occupation: 'Nội trợ', father: 'Trần Văn Cha', mother: 'Lê Thị Mẹ', husband: 'Nguyễn Văn A' },
  { id: 'm3', name: 'Nguyễn Văn Con', avatarUrl: 'https://picsum.photos/seed/member3/50/50', relationship: 'Con trai', gender: 'Male', isRootMember: false, ordinalNumber: 2, birthDeathYears: '1930-2000', occupation: 'Giáo viên', father: 'Nguyễn Văn A', mother: 'Trần Thị X', wife: 'Nguyễn Thị Y' },
  { id: 'm4', name: 'Nguyễn Thị Y', avatarUrl: 'https://picsum.photos/seed/member4/50/50', relationship: 'Vợ', gender: 'Female', isRootMember: false, ordinalNumber: 3, birthDeathYears: '1935-2005', occupation: 'Y tá', father: 'Phạm Văn Cha', mother: 'Đỗ Thị Mẹ', husband: 'Nguyễn Văn Con' },
  { id: 'm5', name: 'Trần Văn D', avatarUrl: 'https://picsum.photos/seed/member5/50/50', relationship: 'Trưởng tộc', gender: 'Male', isRootMember: true, ordinalNumber: 1, birthDeathYears: '1910-1990', occupation: 'Thợ mộc', father: 'Trần Văn Cha', mother: 'Nguyễn Thị Mẹ', wife: 'Lê Thị Y' },
  { id: 'm6', name: 'Lê Thị Y', avatarUrl: 'https://picsum.photos/seed/member6/50/50', relationship: 'Vợ', gender: 'Female', isRootMember: false, birthDeathYears: '1915-1995', occupation: 'Nội trợ', father: 'Lê Văn Cha', mother: 'Phạm Thị Mẹ', husband: 'Trần Văn D' },
  { id: 'm7', name: 'Trần Văn Con 1', avatarUrl: 'https://picsum.photos/seed/member7/50/50', relationship: 'Con trai', gender: 'Male', isRootMember: false, ordinalNumber: 2, birthDeathYears: '1940-', occupation: 'Kỹ sư', father: 'Trần Văn D', mother: 'Lê Thị Y', wife: 'Trần Thị Con 2' },
  { id: 'm8', name: 'Trần Thị Con 2', avatarUrl: 'https://picsum.photos/seed/member8/50/50', relationship: 'Vợ', gender: 'Female', isRootMember: false, ordinalNumber: 3, birthDeathYears: '1945-', occupation: 'Bác sĩ', father: 'Hoàng Văn Cha', mother: 'Võ Thị Mẹ', husband: 'Trần Văn Con 1' },
  { id: 'm9', name: 'Lê Văn C', avatarUrl: 'https://picsum.photos/seed/member9/50/50', relationship: 'Trưởng tộc', gender: 'Male', isRootMember: true, ordinalNumber: 1, birthDeathYears: '1920-2010', occupation: 'Doanh nhân', father: 'Lê Văn Cha', mother: 'Trần Thị Mẹ', wife: 'Phạm Thị Z' },
  { id: 'm10', name: 'Phạm Thị Z', avatarUrl: 'https://picsum.photos/seed/member10/50/50', relationship: 'Vợ', gender: 'Female', isRootMember: false, birthDeathYears: '1925-2015', occupation: 'Nội trợ', father: 'Đỗ Văn Cha', mother: 'Nguyễn Thị Mẹ', husband: 'Lê Văn C' },
];

export const fetchFamilyMembers = async (
  familyId: string,
  query: string,
  filters: MemberFilter,
  page: number,
  itemsPerPage: number,
  signal?: AbortSignal
): Promise<{ data: FamilyMember[]; totalCount: number }> => {
  await new Promise((resolve) => setTimeout(resolve, 500)); // Simulate API delay

  let filteredMembers = mockMembers.filter(member => {
    // Filter by familyId (if needed, currently mockMembers are generic)
    // For now, let's assume all mockMembers belong to the currentFamilyId for simplicity
    // In a real app, you'd filter by familyId first.

    const matchesQuery = member.name.toLowerCase().includes(query.toLowerCase()) ||
                         member.relationship.toLowerCase().includes(query.toLowerCase());

    const matchesGender = filters.gender ? member.gender === filters.gender : true;
    const matchesRootMember = filters.isRootMember !== undefined ? member.isRootMember === filters.isRootMember : true;

    return matchesQuery && matchesGender && matchesRootMember;
  });

  const startIndex = (page - 1) * itemsPerPage;
  const endIndex = startIndex + itemsPerPage;
  const paginatedMembers = filteredMembers.slice(startIndex, endIndex);

  return { data: paginatedMembers, totalCount: filteredMembers.length };
};

export interface MemberDetail extends FamilyMember {
  biography?: string;
  created: string;
  createdBy: string;
  dateOfBirth?: string;
  dateOfDeath?: string;
  familyId: string;
  fatherId?: string;
  firstName: string;
  fullName: string;
  husbandId?: string;
  isRoot: boolean;
  lastModified: string;
  lastModifiedBy: string;
  lastName: string;
  motherId?: string;
  nickname?: string;
  placeOfBirth?: string;
  placeOfDeath?: string;
  wifeId?: string;
}

const detailedMockMembers: MemberDetail[] = [
  {
    id: 'm1',
    name: 'Nguyễn Văn A',
    avatarUrl: 'https://picsum.photos/seed/member1/50/50',
    relationship: 'Trưởng tộc',
    gender: 'Male',
    isRootMember: true,
    ordinalNumber: 1,
    birthDeathYears: '1900-1980',
    occupation: 'Nông dân',
    father: 'Nguyễn Văn Cha',
    mother: 'Trần Thị Mẹ',
    wife: 'Trần Thị X',
    biography: 'Ông Nguyễn Văn A là người đứng đầu dòng họ Nguyễn, có công lớn trong việc gìn giữ gia phả và truyền thống gia đình.',
    created: '2023-01-15T10:00:00Z',
    createdBy: 'd1eae26d-7432-42cf-8c34-2059ee832102',
    dateOfBirth: '1900-01-01T00:00:00Z',
    dateOfDeath: '1980-12-31T00:00:00Z',
    familyId: '1',
    fatherId: 'f1',
    firstName: 'Văn A',
    fullName: 'Nguyễn Văn A',
    husbandId: undefined,
    isRoot: true,
    lastModified: '2024-05-20T14:30:00Z',
    lastModifiedBy: 'd1eae26d-7432-42cf-8c34-2059ee832102',
    lastName: 'Nguyễn',
    motherId: 'm1',
    nickname: 'Cụ Cả',
    placeOfBirth: 'Hà Nội',
    placeOfDeath: 'Hà Nội',
    wifeId: 'm2',
  },
  {
    id: 'm2',
    name: 'Trần Thị X',
    avatarUrl: 'https://picsum.photos/seed/member2/50/50',
    relationship: 'Vợ',
    gender: 'Female',
    isRootMember: false,
    birthDeathYears: '1905-1985',
    occupation: 'Nội trợ',
    father: 'Trần Văn Cha',
    mother: 'Lê Thị Mẹ',
    husband: 'Nguyễn Văn A',
    biography: 'Bà Trần Thị X là vợ của ông Nguyễn Văn A, một người phụ nữ đảm đang, hết lòng vì gia đình.',
    created: '2023-01-15T10:00:00Z',
    createdBy: 'd1eae26d-7432-42cf-8c34-2059ee832102',
    dateOfBirth: '1905-03-10T00:00:00Z',
    dateOfDeath: '1985-07-20T00:00:00Z',
    familyId: '1',
    fatherId: 'f2',
    firstName: 'Thị X',
    fullName: 'Trần Thị X',
    husbandId: 'm1',
    isRoot: false,
    lastModified: '2024-05-20T14:30:00Z',
    lastModifiedBy: 'd1eae26d-7432-42cf-8c34-2059ee832102',
    lastName: 'Trần',
    motherId: 'm2',
    nickname: 'Bà Hai',
    placeOfBirth: 'Hà Nội',
    placeOfDeath: 'Hà Nội',
    wifeId: undefined,
  },
  {
    id: 'm3',
    name: 'Nguyễn Văn Con',
    avatarUrl: 'https://picsum.photos/seed/member3/50/50',
    relationship: 'Con trai',
    gender: 'Male',
    isRootMember: false,
    ordinalNumber: 2,
    birthDeathYears: '1930-2000',
    occupation: 'Giáo viên',
    father: 'Nguyễn Văn A',
    mother: 'Trần Thị X',
    wife: 'Nguyễn Thị Y',
    biography: 'Ông Nguyễn Văn Con là con trai của ông A và bà X, một nhà giáo mẫu mực.',
    created: '2023-01-15T10:00:00Z',
    createdBy: 'd1eae26d-7432-42cf-8c34-2059ee832102',
    dateOfBirth: '1930-05-12T00:00:00Z',
    dateOfDeath: '2000-01-01T00:00:00Z',
    familyId: '1',
    fatherId: 'm1',
    firstName: 'Văn Con',
    fullName: 'Nguyễn Văn Con',
    husbandId: undefined,
    isRoot: false,
    lastModified: '2024-05-20T14:30:00Z',
    lastModifiedBy: 'd1eae26d-7432-42cf-8c34-2059ee832102',
    lastName: 'Nguyễn',
    motherId: 'm2',
    nickname: 'Thầy Giáo',
    placeOfBirth: 'Hà Nội',
    placeOfDeath: 'Hà Nội',
    wifeId: 'm4',
  },
  {
    id: 'm4',
    name: 'Nguyễn Thị Y',
    avatarUrl: 'https://picsum.photos/seed/member4/50/50',
    relationship: 'Vợ',
    gender: 'Female',
    isRootMember: false,
    ordinalNumber: 3,
    birthDeathYears: '1935-2005',
    occupation: 'Y tá',
    father: 'Phạm Văn Cha',
    mother: 'Đỗ Thị Mẹ',
    husband: 'Nguyễn Văn Con',
    biography: 'Bà Nguyễn Thị Y là vợ của ông Nguyễn Văn Con, một y tá tận tụy.',
    created: '2023-01-15T10:00:00Z',
    createdBy: 'd1eae26d-7432-42cf-8c34-2059ee832102',
    dateOfBirth: '1935-08-20T00:00:00Z',
    dateOfDeath: '2005-04-15T00:00:00Z',
    familyId: '1',
    fatherId: 'f3',
    firstName: 'Thị Y',
    fullName: 'Nguyễn Thị Y',
    husbandId: 'm3',
    isRoot: false,
    lastModified: '2024-05-20T14:30:00Z',
    lastModifiedBy: 'd1eae26d-7432-42cf-8c34-2059ee832102',
    lastName: 'Nguyễn',
    motherId: 'm3',
    nickname: 'Cô Y tá',
    placeOfBirth: 'Hà Nội',
    placeOfDeath: 'Hà Nội',
    wifeId: undefined,
  },
  {
    id: 'm5',
    name: 'Trần Văn D',
    avatarUrl: 'https://picsum.photos/seed/member5/50/50',
    relationship: 'Trưởng tộc',
    gender: 'Male',
    isRootMember: true,
    ordinalNumber: 1,
    birthDeathYears: '1910-1990',
    occupation: 'Thợ mộc',
    father: 'Trần Văn Cha',
    mother: 'Nguyễn Thị Mẹ',
    wife: 'Lê Thị Y',
    biography: 'Ông Trần Văn D là người đứng đầu dòng họ Trần, một thợ mộc tài hoa.',
    created: '2023-01-15T10:00:00Z',
    createdBy: 'd1eae26d-7432-42cf-8c34-2059ee832102',
    dateOfBirth: '1910-02-05T00:00:00Z',
    dateOfDeath: '1990-11-11T00:00:00Z',
    familyId: '2',
    fatherId: 'f4',
    firstName: 'Văn D',
    fullName: 'Trần Văn D',
    husbandId: undefined,
    isRoot: true,
    lastModified: '2024-05-20T14:30:00Z',
    lastModifiedBy: 'd1eae26d-7432-42cf-8c34-2059ee832102',
    lastName: 'Trần',
    motherId: 'm4',
    nickname: 'Bác Thợ',
    placeOfBirth: 'Huế',
    placeOfDeath: 'Huế',
    wifeId: 'm6',
  },
  {
    id: 'm6',
    name: 'Lê Thị Y',
    avatarUrl: 'https://picsum.photos/seed/member6/50/50',
    relationship: 'Vợ',
    gender: 'Female',
    isRootMember: false,
    birthDeathYears: '1915-1995',
    occupation: 'Nội trợ',
    father: 'Lê Văn Cha',
    mother: 'Phạm Thị Mẹ',
    husband: 'Trần Văn D',
    biography: 'Bà Lê Thị Y là vợ của ông Trần Văn D, một người phụ nữ hiền thục.',
    created: '2023-01-15T10:00:00Z',
    createdBy: 'd1eae26d-7432-42cf-8c34-2059ee832102',
    dateOfBirth: '1915-06-01T00:00:00Z',
    dateOfDeath: '1995-09-30T00:00:00Z',
    familyId: '2',
    fatherId: 'f5',
    firstName: 'Thị Y',
    fullName: 'Lê Thị Y',
    husbandId: 'm5',
    isRoot: false,
    lastModified: '2024-05-20T14:30:00Z',
    lastModifiedBy: 'd1eae26d-7432-42cf-8c34-2059ee832102',
    lastName: 'Lê',
    motherId: 'm5',
    nickname: 'Mợ Ba',
    placeOfBirth: 'Huế',
    placeOfDeath: 'Huế',
    wifeId: undefined,
  },
  {
    id: 'm7',
    name: 'Trần Văn Con 1',
    avatarUrl: 'https://picsum.photos/seed/member7/50/50',
    relationship: 'Con trai',
    gender: 'Male',
    isRootMember: false,
    ordinalNumber: 2,
    birthDeathYears: '1940-',
    occupation: 'Kỹ sư',
    father: 'Trần Văn D',
    mother: 'Lê Thị Y',
    wife: 'Trần Thị Con 2',
    biography: 'Ông Trần Văn Con 1 là con trai của ông D và bà Y, một kỹ sư giỏi.',
    created: '2023-01-15T10:00:00Z',
    createdBy: 'd1eae26d-7432-42cf-8c34-2059ee832102',
    dateOfBirth: '1940-04-25T00:00:00Z',
    dateOfDeath: undefined,
    familyId: '2',
    fatherId: 'm5',
    firstName: 'Văn Con 1',
    fullName: 'Trần Văn Con 1',
    husbandId: undefined,
    isRoot: false,
    lastModified: '2024-05-20T14:30:00Z',
    lastModifiedBy: 'd1eae26d-7432-42cf-8c34-2059ee832102',
    lastName: 'Trần',
    motherId: 'm6',
    nickname: 'Anh Kỹ sư',
    placeOfBirth: 'Huế',
    placeOfDeath: undefined,
    wifeId: 'm8',
  },
  {
    id: 'm8',
    name: 'Trần Thị Con 2',
    avatarUrl: 'https://picsum.photos/seed/member8/50/50',
    relationship: 'Vợ',
    gender: 'Female',
    isRootMember: false,
    ordinalNumber: 3,
    birthDeathYears: '1945-',
    occupation: 'Bác sĩ',
    father: 'Hoàng Văn Cha',
    mother: 'Võ Thị Mẹ',
    husband: 'Trần Văn Con 1',
    biography: 'Bà Trần Thị Con 2 là vợ của ông Trần Văn Con 1, một bác sĩ tận tâm.',
    created: '2023-01-15T10:00:00Z',
    createdBy: 'd1eae26d-7432-42cf-8c34-2059ee832102',
    dateOfBirth: '1945-10-01T00:00:00Z',
    dateOfDeath: undefined,
    familyId: '2',
    fatherId: 'f6',
    firstName: 'Thị Con 2',
    fullName: 'Trần Thị Con 2',
    husbandId: 'm7',
    isRoot: false,
    lastModified: '2024-05-20T14:30:00Z',
    lastModifiedBy: 'd1eae26d-7432-42cf-8c34-2059ee832102',
    lastName: 'Trần',
    motherId: 'm6',
    nickname: 'Chị Bác sĩ',
    placeOfBirth: 'Huế',
    placeOfDeath: undefined,
    wifeId: undefined,
  },
  {
    id: 'm9',
    name: 'Lê Văn C',
    avatarUrl: 'https://picsum.photos/seed/member9/50/50',
    relationship: 'Trưởng tộc',
    gender: 'Male',
    isRootMember: true,
    ordinalNumber: 1,
    birthDeathYears: '1920-2010',
    occupation: 'Doanh nhân',
    father: 'Lê Văn Cha',
    mother: 'Trần Thị Mẹ',
    wife: 'Phạm Thị Z',
    biography: 'Ông Lê Văn C là người đứng đầu dòng họ Lê, một doanh nhân thành đạt.',
    created: '2023-01-15T10:00:00Z',
    createdBy: 'd1eae26d-7432-42cf-8c34-2059ee832102',
    dateOfBirth: '1920-07-07T00:00:00Z',
    dateOfDeath: '2010-02-02T00:00:00Z',
    familyId: '3',
    fatherId: 'f7',
    firstName: 'Văn C',
    fullName: 'Lê Văn C',
    husbandId: undefined,
    isRoot: true,
    lastModified: '2024-05-20T14:30:00Z',
    lastModifiedBy: 'd1eae26d-7432-42cf-8c34-2059ee832102',
    lastName: 'Lê',
    motherId: 'm7',
    nickname: 'Ông Chủ',
    placeOfBirth: 'Sài Gòn',
    placeOfDeath: 'Sài Gòn',
    wifeId: 'm10',
  },
  {
    id: 'm10',
    name: 'Phạm Thị Z',
    avatarUrl: 'https://picsum.photos/seed/member10/50/50',
    relationship: 'Vợ',
    gender: 'Female',
    isRootMember: false,
    birthDeathYears: '1925-2015',
    occupation: 'Nội trợ',
    father: 'Đỗ Văn Cha',
    mother: 'Nguyễn Thị Mẹ',
    husband: 'Lê Văn C',
    biography: 'Bà Phạm Thị Z là vợ của ông Lê Văn C, một người phụ nữ của gia đình.',
    created: '2023-01-15T10:00:00Z',
    createdBy: 'd1eae26d-7432-42cf-8c34-2059ee832102',
    dateOfBirth: '1925-11-11T00:00:00Z',
    dateOfDeath: '2015-03-03T00:00:00Z',
    familyId: '3',
    fatherId: 'f8',
    firstName: 'Thị Z',
    fullName: 'Phạm Thị Z',
    husbandId: 'm9',
    isRoot: false,
    lastModified: '2024-05-20T14:30:00Z',
    lastModifiedBy: 'd1eae26d-7432-42cf-8c34-2059ee832102',
    lastName: 'Phạm',
    motherId: 'm8',
    nickname: 'Bà Chủ',
    placeOfBirth: 'Sài Gòn',
    placeOfDeath: 'Sài Gòn',
    wifeId: undefined,
  },
];

export const fetchMemberDetails = async (memberId: string): Promise<MemberDetail | null> => {
  await new Promise((resolve) => setTimeout(resolve, 500)); // Simulate API delay
  return detailedMockMembers.find((member) => member.id === memberId) || null;
};
