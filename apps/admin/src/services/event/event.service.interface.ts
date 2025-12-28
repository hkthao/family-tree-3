import type { EventDto, AddEventDto, UpdateEventDto } from "@/types";
import type { Result, ApiError } from '@/types';
import type { ICrudService } from "../common/crud.service.interface";

export interface IEventService extends ICrudService<EventDto, AddEventDto, UpdateEventDto> {
  getEventsByFamilyId(familyId: string): Promise<Result<EventDto[]>>;
  exportEvents(familyId?: string): Promise<Result<string, ApiError>>;
  importEvents(familyId: string, payload: any): Promise<Result<void, ApiError>>;
}