import type { Event } from "@/types";
import type { Result } from '@/types';
import type { ICrudService } from "../common/crud.service.interface";

export interface IEventService extends ICrudService<Event> {
  getEventsByFamilyId(familyId: string): Promise<Result<Event[]>>;
}