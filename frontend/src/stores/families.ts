import { defineStore } from 'pinia';
import { ref } from 'vue';
import apiClient from '../plugins/axios';
import type { Family } from '../types/family';

export const useFamiliesStore = defineStore('families', () => {


  const items = ref<Family[]>([]);
  const loading = ref(false);
  const total = ref(0);
  const error = ref<string | null>(null);

  const VITE_USE_MOCK = import.meta.env.VITE_USE_MOCK === 'true';

  async function fetchAll(search?: string, page: number = 1, perPage: number = 10) {
    loading.value = true;
    error.value = null;
    try {
      if (VITE_USE_MOCK) {
        const { familiesMock } = await import('../data/mock/families.mock');
        // Simulate API call delay
        await new Promise(resolve => setTimeout(resolve, 500));
        items.value = familiesMock.slice((page - 1) * perPage, page * perPage);
        total.value = familiesMock.length;
      } else {
        const response = await apiClient.get('/api/families', { params: { search, page, perPage } });
        items.value = response.data.items;
        total.value = response.data.total;
      }
    } catch (e: any) {
      error.value = e.message || 'Failed to fetch families';
    } finally {
      loading.value = false;
    }
  }

  async function add(entity: Family) {
    loading.value = true;
    error.value = null;
    try {
      if (VITE_USE_MOCK) {
        const { familiesMock } = await import('../data/mock/families.mock');
        await new Promise(resolve => setTimeout(resolve, 500));
        const newFamily = { ...entity, id: (familiesMock.length + 1).toString() };
        familiesMock.push(newFamily);
        items.value.push(newFamily);
        total.value++;
      } else {
        const response = await apiClient.post('/api/families', entity);
        items.value.push(response.data);
        total.value++;
      }
    } catch (e: any) {
      error.value = e.message || 'Failed to add family';
    } finally {
      loading.value = false;
    }
  }

  async function update(entity: Family) {
    loading.value = true;
    error.value = null;
    try {
      if (VITE_USE_MOCK) {
        const { familiesMock } = await import('../data/mock/families.mock');
        await new Promise(resolve => setTimeout(resolve, 500));
        const index = familiesMock.findIndex(f => f.id === entity.id);
        if (index !== -1) {
          familiesMock[index] = entity;
          const itemIndex = items.value.findIndex(f => f.id === entity.id);
          if (itemIndex !== -1) {
            items.value[itemIndex] = entity;
          }
        }
      } else {
        const response = await apiClient.put(`/api/families/${entity.id}`, entity);
        const itemIndex = items.value.findIndex(f => f.id === entity.id);
        if (itemIndex !== -1) {
          items.value[itemIndex] = response.data;
        }
      }
    } catch (e: any) {
      error.value = e.message || 'Failed to update family';
    } finally {
      loading.value = false;
    }
  }

  async function remove(id: string) {
    loading.value = true;
    error.value = null;
    try {
      if (VITE_USE_MOCK) {
        let { familiesMock } = await import('../data/mock/families.mock');
        await new Promise(resolve => setTimeout(resolve, 500));
        familiesMock = familiesMock.filter(f => f.id !== id);
        items.value = items.value.filter(f => f.id !== id);
        total.value--;
      } else {
        await apiClient.delete(`/api/families/${id}`);
        items.value = items.value.filter(f => f.id !== id);
        total.value--;
      }
    } catch (e: any) {
      error.value = e.message || 'Failed to delete family';
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
