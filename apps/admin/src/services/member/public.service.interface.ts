import type { ApiError, Member, Result } from "@/types";

export interface IPublicMemberService {
  getPublicMembersByFamilyId(familyId: string): Promise<Result<Member[], ApiError>>;
  getPublicMemberById(id: string, familyId: string): Promise<Result<Member | undefined, ApiError>>;
}
