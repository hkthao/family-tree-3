import { ref } from 'vue';

import type { Family } from '@/types/family';
import { faker } from '@faker-js/faker';

const generateMockFamilies = (count: number): Family[] => {
  const families: Family[] = [];
  const visibilityOptions = ['Private', 'Public'];

  for (let i = 0; i < count; i++) {
    families.push({
      id: i + 1,
      name: faker.person.lastName() + ' Family',
      description: faker.lorem.sentence(),
      address: faker.location.streetAddress() + ', ' + faker.location.city() + ', ' + faker.location.country(),
      avatarUrl: faker.image.avatar(),
      visibility: faker.helpers.arrayElement(visibilityOptions) as Family['visibility'],
    });
  }
  return families;
};

const families = ref<Family[]>(generateMockFamilies(10));

export const useFamilies = () => {
  const getFamilies = (
    search: string = '',
    visibility: 'Private' | 'Public' | 'All' = 'All',
    page: number = 1,
    itemsPerPage: number = 5
  ) => {
    return new Promise<{ families: Family[]; total: number }>((resolve) => {
      setTimeout(() => {
        let filteredFamilies = families.value;

        if (search) {
          filteredFamilies = filteredFamilies.filter((f) =>
            f.name.toLowerCase().includes(search.toLowerCase())
          );
        }

        if (visibility !== 'All') {
          filteredFamilies = filteredFamilies.filter(
            (f) => f.visibility === visibility
          );
        }

        const start = (page - 1) * itemsPerPage;
        const end = start + itemsPerPage;
        const paginatedFamilies = filteredFamilies.slice(start, end);

        resolve({ families: paginatedFamilies, total: filteredFamilies.length });
      }, 500); // Simulate network delay
    });
  };

  const getFamilyById = (id: number) => {
    return new Promise<Family | undefined>((resolve) => {
      setTimeout(() => {
        resolve(families.value.find((f) => f.id === id));
      }, 300);
    });
  };

  const addFamily = (newFamily: Omit<Family, 'id'>) => {
    return new Promise<Family>((resolve) => {
      setTimeout(() => {
        const id = Math.max(...families.value.map((f) => f.id)) + 1;
        const familyWithId = { ...newFamily, id };
        families.value.push(familyWithId);
        resolve(familyWithId);
      }, 300);
    });
  };

  const updateFamily = (updatedFamily: Family) => {
    return new Promise<Family | undefined>((resolve) => {
      setTimeout(() => {
        const index = families.value.findIndex((f) => f.id === updatedFamily.id);
        if (index !== -1) {
          families.value[index] = updatedFamily;
          resolve(updatedFamily);
        } else {
          resolve(undefined);
        }
      }, 300);
    });
  };

  const deleteFamily = (id: number) => {
    return new Promise<boolean>((resolve) => {
      setTimeout(() => {
        const initialLength = families.value.length;
        families.value = families.value.filter((f) => f.id !== id);
        resolve(families.value.length < initialLength);
      }, 300);
    });
  };

  return {
    getFamilies,
    getFamilyById,
    addFamily,
    updateFamily,
    deleteFamily,
  };
};
