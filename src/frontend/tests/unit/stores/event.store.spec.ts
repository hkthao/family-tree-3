import { useEventStore } from '@/stores/event.store';
import { defineCrudTests } from '../../shared/crudTests';
import { MockEventService } from '../../shared/mock.services';
import type { Event } from '@/types';
import { EventType } from '@/types';

const mockEventService = new MockEventService();

const entitySample: Event = {
  id: 'event-001',
  familyId: 'family-001',
  name: 'Test Event',
  description: 'An event for testing',
  type: EventType.Other,
  startDate: new Date(),
};

defineCrudTests(
  'Event Store',
  useEventStore,
  mockEventService,
  'event',
  entitySample as Event & { id: string },
);