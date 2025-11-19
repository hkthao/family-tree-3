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
  parents?: string[];
  spouse?: string;
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
  { id: 'm1', name: 'Nguyễn Văn A', avatarUrl: 'https://picsum.photos/seed/member1/50/50', relationship: 'Trưởng tộc', gender: 'Male', isRootMember: true, ordinalNumber: 1, birthDeathYears: '1900-1980', occupation: 'Nông dân', parents: [], spouse: 'Trần Thị X' },
  { id: 'm2', name: 'Trần Thị X', avatarUrl: 'https://picsum.photos/seed/member2/50/50', relationship: 'Vợ', gender: 'Female', isRootMember: false, birthDeathYears: '1905-1985', occupation: 'Nội trợ', parents: [], spouse: 'Nguyễn Văn A' },
  { id: 'm3', name: 'Nguyễn Văn Con', avatarUrl: 'https://picsum.photos/seed/member3/50/50', relationship: 'Con trai', gender: 'Male', isRootMember: false, ordinalNumber: 2, birthDeathYears: '1930-2000', occupation: 'Giáo viên', parents: ['Nguyễn Văn A', 'Trần Thị X'], spouse: 'Nguyễn Thị Y' },
  { id: 'm4', name: 'Nguyễn Thị Y', avatarUrl: 'https://picsum.photos/seed/member4/50/50', relationship: 'Vợ', gender: 'Female', isRootMember: false, ordinalNumber: 3, birthDeathYears: '1935-2005', occupation: 'Y tá', parents: [], spouse: 'Nguyễn Văn Con' },
  { id: 'm5', name: 'Trần Văn D', avatarUrl: 'https://picsum.photos/seed/member5/50/50', relationship: 'Trưởng tộc', gender: 'Male', isRootMember: true, ordinalNumber: 1, birthDeathYears: '1910-1990', occupation: 'Thợ mộc', parents: [], spouse: 'Lê Thị Y' },
  { id: 'm6', name: 'Lê Thị Y', avatarUrl: 'https://picsum.photos/seed/member6/50/50', relationship: 'Vợ', gender: 'Female', isRootMember: false, birthDeathYears: '1915-1995', occupation: 'Nội trợ', parents: [], spouse: 'Trần Văn D' },
  { id: 'm7', name: 'Trần Văn Con 1', avatarUrl: 'https://picsum.photos/seed/member7/50/50', relationship: 'Con trai', gender: 'Male', isRootMember: false, ordinalNumber: 2, birthDeathYears: '1940-', occupation: 'Kỹ sư', parents: ['Trần Văn D', 'Lê Thị Y'], spouse: 'Trần Thị Con 2' },
  { id: 'm8', name: 'Trần Thị Con 2', avatarUrl: 'https://picsum.photos/seed/member8/50/50', relationship: 'Vợ', gender: 'Female', isRootMember: false, ordinalNumber: 3, birthDeathYears: '1945-', occupation: 'Bác sĩ', parents: [], spouse: 'Trần Văn Con 1' },
  { id: 'm9', name: 'Lê Văn C', avatarUrl: 'https://picsum.photos/seed/member9/50/50', relationship: 'Trưởng tộc', gender: 'Male', isRootMember: true, ordinalNumber: 1, birthDeathYears: '1920-2010', occupation: 'Doanh nhân', parents: [], spouse: 'Phạm Thị Z' },
  { id: 'm10', name: 'Phạm Thị Z', avatarUrl: 'https://picsum.photos/seed/member10/50/50', relationship: 'Vợ', gender: 'Female', isRootMember: false, birthDeathYears: '1925-2015', occupation: 'Nội trợ', parents: [], spouse: 'Lê Văn C' },
];

export const fetchFamilyMembers = async (
  familyId: string,
  query: string,
  filters: MemberFilter,
  page: number,
  pageSize: number,
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

  const startIndex = (page - 1) * pageSize;
  const endIndex = startIndex + pageSize;
  const paginatedMembers = filteredMembers.slice(startIndex, endIndex);

  return { data: paginatedMembers, totalCount: filteredMembers.length };
};

export const fetchFamilyEvents = async (familyId: string): Promise<FamilyEvent[]> => {
  await new Promise((resolve) => setTimeout(resolve, 500)); // Simulate API delay
  // Mock events for family 1
  if (familyId === '1') {
    return [
      { id: 'e1', name: 'Giỗ tổ', date: '2024-08-15T00:00:00Z', description: 'Lễ giỗ tổ hàng năm của gia đình' },
      { id: 'e2', name: 'Họp mặt gia đình', date: '2024-12-25T00:00:00Z', description: 'Buổi họp mặt cuối năm' },
    ];
  }
  return [];
};
