import type { EventDto, AddEventDto, UpdateEventDto, GenerateAndNotifyEventsCommand } from "@/types";
import type { Result, ApiError } from '@/types';
import type { ICrudService } from "../common/crud.service.interface";

export interface IEventService extends ICrudService<EventDto, AddEventDto, UpdateEventDto> {
  getEventsByFamilyId(familyId: string): Promise<Result<EventDto[]>>;
  getEventsByMemberId(memberId: string): Promise<Result<EventDto[], ApiError>>;
  exportEvents(familyId?: string): Promise<Result<string, ApiError>>;
  importEvents(familyId: string, payload: any): Promise<Result<void, ApiError>>;
  generateAndNotifyEvents(command: GenerateAndNotifyEventsCommand): Promise<Result<string, ApiError>>;
}