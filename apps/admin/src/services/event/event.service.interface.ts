import type { EventDto, AddEventDto, UpdateEventDto } from "@/types";
import type { Result } from '@/types';
import type { ICrudService } from "../common/crud.service.interface";

export interface IEventService extends ICrudService<EventDto> {
  getEventsByFamilyId(familyId: string): Promise<Result<EventDto[]>>;
}