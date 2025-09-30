import type { Member, Result, MemberFilter, Paginated } from "@/types";
import type { ApiError } from "@/utils/api";
import type { ICrudService } from "../common/crud.service.interface";

export interface IMemberService extends ICrudService<Member> { // Extend ICrudService
  fetchMembersByFamilyId(familyId: string): Promise<Result<Member[], ApiError>>; // Keep specific method
  loadItems(filters: MemberFilter, page: number, itemsPerPage: number): Promise<Result<Paginated<Member>, ApiError>>; // Keep specific method
}
