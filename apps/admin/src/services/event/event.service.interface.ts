import type { Event } from "@/types";
import type { ICrudService } from "../common/crud.service.interface";

export interface IEventService extends ICrudService<Event> {
  // Event-specific methods can be added here if needed in the future
}