import type { Member, Result } from "@/types";
import type { ApiError } from "@/plugins/axios";

export interface IPublicMemberService {
  getPublicMembersByFamilyId(familyId: string): Promise<Result<Member[], ApiError>>;
  getPublicMemberById(id: string, familyId: string): Promise<Result<Member, ApiError>>;
}
