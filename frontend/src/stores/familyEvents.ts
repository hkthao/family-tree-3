import { defineStore } from 'pinia';
import { ref } from 'vue';
import type { FamilyEvent, FamilyEventServiceType } from '../services/familyEvent.service';

export const useFamilyEventsStore = defineStore('familyEvents', (familyEventService: FamilyEventServiceType) => {
  const familyEvents = ref<FamilyEvent[]>([]);
  const loading = ref(false);
  const error = ref<string | null>(null);
  const total = ref(0);

  async function fetchAll(search = '', familyId?: string, page = 1, perPage = 10) {
    loading.value = true;
    error.value = null;
    try {
      const response = await familyEventService.fetchFamilyEvents(search, familyId, page, perPage);
      familyEvents.value = response.items;
      total.value = response.total;
    } catch (err: any) {
      error.value = err.message || 'Failed to fetch family events';
      console.error(err);
    } finally {
      loading.value = false;
    }
  }

  async function fetchOne(id: string): Promise<FamilyEvent | undefined> {
    loading.value = true;
    error.value = null;
    try {
      const event = await familyEventService.fetchFamilyEventById(id);
      return event;
    } catch (err: any) {
      error.value = err.message || 'Failed to fetch family event';
      console.error(err);
      return undefined;
    } finally {
      loading.value = false;
    }
  }

  async function add(event: Omit<FamilyEvent, 'id' | 'createdAt' | 'updatedAt'>) {
    loading.value = true;
    error.value = null;
    try {
      const newEvent = await familyEventService.addFamilyEvent(event);
      familyEvents.value.push(newEvent);
      total.value++;
    } catch (err: any) {
      error.value = err.message || 'Failed to add family event';
      console.error(err);
      throw err;
    } finally {
      loading.value = false;
    }
  }

  async function update(id: string, event: Partial<Omit<FamilyEvent, 'id' | 'createdAt' | 'updatedAt'>>) {
    loading.value = true;
    error.value = null;
    try {
      await familyEventService.updateFamilyEvent(id, event);
      const index = familyEvents.value.findIndex(e => e.id === id);
      if (index !== -1) {
        familyEvents.value[index] = { ...familyEvents.value[index], ...event, updatedAt: new Date().toISOString() };
      }
    } catch (err: any) {
      error.value = err.message || 'Failed to update family event';
      console.error(err);
      throw err;
    } finally {
      loading.value = false;
    }
  }

  async function remove(id: string) {
    loading.value = true;
    error.value = null;
    try {
      await familyEventService.removeFamilyEvent(id);
      familyEvents.value = familyEvents.value.filter(e => e.id !== id);
      total.value--;
    } catch (err: any) {
      error.value = err.message || 'Failed to delete family event';
      console.error(err);
      throw err;
    } finally {
      loading.value = false;
    }
  }

  return {
    familyEvents,
    loading,
    error,
    total,
    fetchAll,
    fetchOne,
    add,
    update,
    remove,
  };
});