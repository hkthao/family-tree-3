import type { Result, Member } from '@/types';

export interface IFaceMemberService {
  getManagedMembers(): Promise<Result<Member[], Error>>;
}
