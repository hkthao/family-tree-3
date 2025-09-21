import { defineStore } from 'pinia';
import { ref } from 'vue';
import type { Family, FamilyServiceType } from '../services/family.service';

export const useFamiliesStore = defineStore('families', (familyService: FamilyServiceType) => {
  const families = ref<Family[]>([]);
  const loading = ref(false);
  const error = ref<string | null>(null);
  const total = ref(0);

  async function fetchAll(search = '', page = 1, perPage = 10) {
    loading.value = true;
    error.value = null;
    try {
      const response = await familyService.fetchFamilies(search, page, perPage);
      families.value = response.items;
      total.value = response.total;
    } catch (err: any) {
      error.value = err.message || 'Failed to fetch families';
      console.error(err);
    } finally {
      loading.value = false;
    }
  }

  async function fetchOne(id: string): Promise<Family | undefined> {
    loading.value = true;
    error.value = null;
    try {
      const family = await familyService.fetchFamilyById(id);
      return family;
    } catch (err: any) {
      error.value = err.message || 'Failed to fetch family';
      console.error(err);
      return undefined;
    } finally {
      loading.value = false;
    }
  }

  async function add(family: Omit<Family, 'id' | 'createdAt' | 'updatedAt'>) {
    loading.value = true;
    error.value = null;
    try {
      const newFamily = await familyService.addFamily(family);
      families.value.push(newFamily);
      total.value++;
    } catch (err: any) {
      error.value = err.message || 'Failed to add family';
      console.error(err);
      throw err; // Re-throw to allow component to handle
    } finally {
      loading.value = false;
    }
  }

  async function update(id: string, family: Partial<Omit<Family, 'id' | 'createdAt' | 'updatedAt'>>) {
    loading.value = true;
    error.value = null;
    try {
      await familyService.updateFamily(id, family);
      // Update the item in the store's array
      const index = families.value.findIndex(f => f.id === id);
      if (index !== -1) {
        families.value[index] = { ...families.value[index], ...family, updatedAt: new Date().toISOString() };
      }
    } catch (err: any) {
      error.value = err.message || 'Failed to update family';
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
      await familyService.removeFamily(id);
      families.value = families.value.filter(f => f.id !== id);
      total.value--;
    } catch (err: any) {
      error.value = err.message || 'Failed to delete family';
      console.error(err);
      throw err;
    } finally {
      loading.value = false;
    }
  }

  return {
    families,
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
