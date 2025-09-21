import { defineStore } from 'pinia';
import { ref } from 'vue';

import type { FamilyEvent } from '../types/event';

export const useFamilyEventsStore = defineStore('familyEvents', () => {
  const items = ref<FamilyEvent[]>([]);
  const loading = ref(false);
  const total = ref(0);
  const error = ref<string | null>(null);

  const VITE_USE_MOCK = import.meta.env.VITE_USE_MOCK === 'true';

  async function fetchAll(search?: string, page: number = 1, perPage: number = 10) {
    loading.value = true;
    error.value = null;
    try {
      if (VITE_USE_MOCK) {
        const { familyEventsMock } = await import('../data/mock/familyEvents.mock');
        await new Promise(resolve => setTimeout(resolve, 500));
        items.value = familyEventsMock.slice((page - 1) * perPage, page * perPage);
        total.value = familyEventsMock.length;
      } else {
        // TODO: Implement real API call using axios
        console.warn('Real API call for family events not yet implemented.');
        items.value = [];
        total.value = 0;
      }
    } catch (e: any) {
      error.value = e.message || 'Failed to fetch family events';
    } finally {
      loading.value = false;
    }
  }

  async function add(entity: FamilyEvent) {
    loading.value = true;
    error.value = null;
    try {
      if (VITE_USE_MOCK) {
        const { familyEventsMock } = await import('../data/mock/familyEvents.mock');
        await new Promise(resolve => setTimeout(resolve, 500));
        const newEvent = { ...entity, id: (familyEventsMock.length + 1).toString() };
        familyEventsMock.push(newEvent);
        items.value.push(newEvent);
        total.value++;
      } else {
        // TODO: Implement real API call
        console.warn('Real API call for adding family event not yet implemented.');
      }
    } catch (e: any) {
      error.value = e.message || 'Failed to add family event';
    } finally {
      loading.value = false;
    }
  }

  async function update(entity: FamilyEvent) {
    loading.value = true;
    error.value = null;
    try {
      if (VITE_USE_MOCK) {
        const { familyEventsMock } = await import('../data/mock/familyEvents.mock');
        await new Promise(resolve => setTimeout(resolve, 500));
        const index = familyEventsMock.findIndex(e => e.id === entity.id);
        if (index !== -1) {
          familyEventsMock[index] = entity;
          const itemIndex = items.value.findIndex(e => e.id === entity.id);
          if (itemIndex !== -1) {
            items.value[itemIndex] = entity;
          }
        }
      } else {
        // TODO: Implement real API call
        console.warn('Real API call for updating family event not yet implemented.');
      }
    } catch (e: any) {
      error.value = e.message || 'Failed to update family event';
    } finally {
      loading.value = false;
    }
  }

  async function remove(id: string) {
    loading.value = true;
    error.value = null;
    try {
      if (VITE_USE_MOCK) {
        let { familyEventsMock } = await import('../data/mock/familyEvents.mock');
        await new Promise(resolve => setTimeout(resolve, 500));
        familyEventsMock = familyEventsMock.filter(e => e.id !== id);
        items.value = items.value.filter(e => e.id !== id);
        total.value--;
      }
    } catch (e: any) {
      error.value = e.message || 'Failed to delete family event';
    } finally {
      loading.value = false;
    }
  }

  return {
    items,
    loading,
    total,
    error,
    fetchAll,
    add,
    update,
    remove,
  };
});
