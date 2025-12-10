import type { Event } from '@/types'; 
import type { ICrudService } from '../common/crud.service.interface';
import type { Result } from '@/types'; 

export interface IEventService extends ICrudService<Event> {
  getUpcomingEvents(familyId?: string): Promise<Result<Event[]>>; 
  addItems(newItems: Omit<Event, 'id'>[]): Promise<Result<string[]>>; 
}
