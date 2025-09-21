import { ref, computed } from 'vue';
import type { Event, EventFilter } from '@/types/event';
import { faker } from '@faker-js/faker';
import { mockMembers } from './members';

const generateMockEvents = (count: number): Event[] => {
  const events: Event[] = [
    {
      id: faker.string.uuid(),
      name: 'Family Reunion',
      type: 'Other',
      familyId: 1,
      startDate: new Date('2025-09-15T10:00:00'),
      endDate: new Date('2025-09-15T18:00:00'),
      location: "Grandma's House",
      description: 'Annual family gathering.',
      color: '#FF5722',
      relatedMembers: [],
    },
    {
      id: faker.string.uuid(),
      name: "Uncle Bob's Birthday",
      type: 'Birth',
      familyId: 1,
      startDate: new Date('2025-09-20T00:00:00'),
      endDate: new Date('2025-09-20T23:59:59'),
      location: 'Local Restaurant',
      description: "Celebrating Uncle Bob's birthday.",
      color: '#4CAF50',
      relatedMembers: [],
    },
    {
      id: faker.string.uuid(),
      name: 'Wedding Anniversary',
      type: 'Marriage',
      familyId: 2,
      startDate: new Date('2025-09-05T00:00:00'),
      endDate: new Date('2025-09-05T23:59:59'),
      location: 'City Hall',
      description: "John and Jane's wedding anniversary.",
      color: '#2196F3',
      relatedMembers: [],
    },
  ];
  const eventTypes = ['Birth', 'Marriage', 'Death', 'Migration', 'Other'];
  const colors = ['#FFC107', '#4CAF50', '#2196F3', '#FF5722', '#9C27B0', '#673AB7', '#3F51B5', '#03A9F4', '#00BCD4', '#009688'];

  const memberIds = mockMembers.value.map(m => m.id);

  for (let i = 0; i < count; i++) {
    const startDate = faker.date.past({ years: 20 });
    const endDate = faker.datatype.boolean() ? faker.date.soon({ refDate: startDate }) : undefined;
    const type = faker.helpers.arrayElement(eventTypes) as Event['type'];

    events.push({
      id: faker.string.uuid(),
      name: faker.lorem.words(3),
      type: type,
      familyId: faker.helpers.arrayElement([1, 2, 3, 4, 5, 6, 7, 8, 9, 10]),
      startDate: startDate,
      endDate: endDate,
      location: faker.location.city() + ', ' + faker.location.country(),
      description: faker.lorem.paragraph(),
      color: faker.helpers.arrayElement(colors),
      relatedMembers: faker.helpers.arrayElements(memberIds, { min: 0, max: 3 }),
    });
  }
  return events;
};

const mockEvents = ref<Event[]>(generateMockEvents(20));

export function useEvents() {
  const events = ref<Event[]>(mockEvents.value);

  const getEvents = (filter: EventFilter = {}, page = 1, itemsPerPage = 10) => {
    let filtered = events.value;

    if (filter.name) {
      filtered = filtered.filter(event =>
        event.name.toLowerCase().includes(filter.name!.toLowerCase())
      );
    }
    if (filter.type) {
      filtered = filtered.filter(event => event.type === filter.type);
    }
    if (filter.familyId) {
      filtered = filtered.filter(event => event.familyId === filter.familyId);
    }
    if (filter.location) {
      filtered = filtered.filter(event =>
        event.location && event.location.toLowerCase().includes(filter.location!.toLowerCase())
      );
    }
    // Add more filters as needed

    const total = filtered.length;
    const start = (page - 1) * itemsPerPage;
    const end = start + itemsPerPage;
    const paginated = filtered.slice(start, end);

    return { events: paginated, total };
  };

  const getEventById = (id: string) => {
    return mockEvents.value.find(event => event.id === id);
  };

  const addEvent = (newEvent: Omit<Event, 'id'>) => {
    const eventWithId = { ...newEvent, id: Date.now().toString() };
    events.value.push(eventWithId);
    return eventWithId;
  };

  const updateEvent = (updatedEvent: Event) => {
    const index = events.value.findIndex(event => event.id === updatedEvent.id);
    if (index !== -1) {
      events.value[index] = updatedEvent;
    }
    return updatedEvent;
  };

  const deleteEvent = (id: string) => {
    const initialLength = events.value.length;
    events.value = events.value.filter(event => event.id !== id);
    return events.value.length < initialLength; // true if deleted
  };

  return {
    events: computed(() => events.value),
    getEvents,
    getEventById,
    addEvent,
    updateEvent,
    deleteEvent,
  };
}