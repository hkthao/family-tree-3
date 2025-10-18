import type { Result, Member } from '@/types';
import type { IFaceMemberService } from './faceMember.service.interface';

export class MockFaceMemberService implements IFaceMemberService {
  async getManagedMembers(): Promise<Result<Member[], Error>> {
    // Simulate API call delay
    await new Promise(resolve => setTimeout(resolve, 500));

    const mockMembers: Member[] = [
      { id: 'member1', fullName: 'Nguyễn Văn A', lastName: 'Nguyễn', firstName: 'Văn A', familyId: 'family1', avatarUrl: 'https://via.placeholder.com/150/FF0000/FFFFFF?text=A' },
      { id: 'member2', fullName: 'Trần Thị B', lastName: 'Trần', firstName: 'Thị B', familyId: 'family1', avatarUrl: 'https://via.placeholder.com/150/0000FF/FFFFFF?text=B' },
      { id: 'member3', fullName: 'Lê Văn C', lastName: 'Lê', firstName: 'Văn C', familyId: 'family2', avatarUrl: 'https://via.placeholder.com/150/00FF00/FFFFFF?text=C' },
      { id: 'member4', fullName: 'Phạm Thị D', lastName: 'Phạm', firstName: 'Thị D', familyId: 'family2', avatarUrl: 'https://via.placeholder.com/150/FFFF00/000000?text=D' },
    ];
    return { ok: true, value: mockMembers };
  }
}
