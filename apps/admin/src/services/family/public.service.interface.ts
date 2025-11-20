import type { Family, Result } from "@/types";
import type { ApiError } from "@/plugins/axios";

export interface IPublicFamilyService {
  getPublicFamilyById(id: string): Promise<Result<Family, ApiError>>;
}
