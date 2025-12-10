import type { ApiError, Family, Result } from "@/types";

export interface IPublicFamilyService {
  getPublicFamilyById(id: string): Promise<Result<Family, ApiError>>;
}
