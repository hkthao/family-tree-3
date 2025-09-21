import { defineStore } from 'pinia';
import { ref } from 'vue';
import apiClient from '../plugins/axios';

import type { Member } from '../types/member';

export const useMembersStore = defineStore('members', () => {
  const items = ref<Member[]>([]);
  const loading = ref(false);
  const total = ref(0);
  const error = ref<string | null>(null);

  const VITE_USE_MOCK = import.meta.env.VITE_USE_MOCK === 'true';

  async function fetchAll(search?: string, page: number = 1, perPage: number = 10) {
    loading.value = true;
    error.value = null;
    try {
      if (VITE_USE_MOCK) {
        const { membersMock } = await import('../data/mock/members.mock');
        await new Promise(resolve => setTimeout(resolve, 500));
        items.value = membersMock.slice((page - 1) * perPage, page * perPage);
        total.value = membersMock.length;
      } else {
        const response = await apiClient.get('/api/members', { params: { search, page, perPage } });
        items.value = response.data.items;
        total.value = response.data.total;
      }
    } catch (e: any) {
      error.value = e.message || 'Failed to fetch members';
    } finally {
      loading.value = false;
    }
  }

  async function add(entity: Member) {
    loading.value = true;
    error.value = null;
    try {
      if (VITE_USE_MOCK) {
        const { membersMock } = await import('../data/mock/members.mock');
        await new Promise(resolve => setTimeout(resolve, 500));
        const newMember = { ...entity, id: (membersMock.length + 1).toString() };
        membersMock.push(newMember);
        items.value.push(newMember);
        total.value++;
      } else {
        const response = await apiClient.post('/api/members', entity);
        items.value.push(response.data);
        total.value++;
      }
    } catch (e: any) {
      error.value = e.message || 'Failed to add member';
    } finally {
      loading.value = false;
    }
  }

  async function update(entity: Member) {
    loading.value = true;
    error.value = null;
    try {
      if (VITE_USE_MOCK) {
        const { membersMock } = await import('../data/mock/members.mock');
        await new Promise(resolve => setTimeout(resolve, 500));
        const index = membersMock.findIndex(m => m.id === entity.id);
        if (index !== -1) {
          membersMock[index] = entity;
          const itemIndex = items.value.findIndex(m => m.id === entity.id);
          if (itemIndex !== -1) {
            items.value[itemIndex] = entity;
          }
        }
      } else {
        const response = await apiClient.put(`/api/members/${entity.id}`, entity);
        const itemIndex = items.value.findIndex(m => m.id === entity.id);
        if (itemIndex !== -1) {
          items.value[itemIndex] = response.data;
        }
      }
    } catch (e: any) {
      error.value = e.message || 'Failed to update member';
    } finally {
      loading.value = false;
    }
  }

  async function remove(id: string) {
    loading.value = true;
    error.value = null;
    try {
      if (VITE_USE_MOCK) {
        let { membersMock } = await import('../data/mock/members.mock');
        await new Promise(resolve => setTimeout(resolve, 500));
        membersMock = membersMock.filter(m => m.id !== id);
        items.value = items.value.filter(m => m.id !== id);
        total.value--;
      }
    } catch (e: any) {
      error.value = e.message || 'Failed to delete member';
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
