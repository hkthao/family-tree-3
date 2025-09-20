import { ref } from 'vue';

import type { Family } from '@/types/family';

const families = ref<Family[]>([
  {
    id: 1,
    name: 'Huynh Family',
    description: 'A large and loving family.',
    avatarUrl: 'https://cdn.vuetifyjs.com/images/cards/halcyon.png',
    visibility: 'Public',
  },
  {
    id: 2,
    name: 'Nguyen Family',
    description: 'Close-knit family with strong traditions.',
    avatarUrl: 'https://cdn.vuetifyjs.com/images/cards/sunshine.jpg',
    visibility: 'Private',
  },
  {
    id: 3,
    name: 'Tran Family',
    description: 'Modern family with diverse interests.',
    avatarUrl: 'https://cdn.vuetifyjs.com/images/cards/docks.jpg',
    visibility: 'Public',
  },
  {
    id: 4,
    name: 'Le Family',
    description: 'Small family, big dreams.',
    avatarUrl: 'https://cdn.vuetifyjs.com/images/cards/road.jpg',
    visibility: 'Private',
  },
  {
    id: 5,
    name: 'Pham Family',
    description: 'Family of adventurers.',
    avatarUrl: 'https://cdn.vuetifyjs.com/images/cards/forest.jpg',
    visibility: 'Public',
  },
  {
    id: 6,
    name: 'Vo Family',
    description: 'Artistic and creative family.',
    avatarUrl: 'https://cdn.vuetifyjs.com/images/cards/plane.jpg',
    visibility: 'Private',
  },
  {
    id: 7,
    name: 'Hoang Family',
    description: 'Family of scholars.',
    avatarUrl: 'https://cdn.vuetifyjs.com/images/cards/house.jpg',
    visibility: 'Public',
  },
  {
    id: 8,
    name: 'Dang Family',
    description: 'Musical family.',
    avatarUrl: 'https://cdn.vuetifyjs.com/images/cards/server-room.jpg',
    visibility: 'Private',
  },
  {
    id: 9,
    name: 'Bui Family',
    description: 'Family of food lovers.',
    avatarUrl: 'https://cdn.vuetifyjs.com/images/cards/cooking.png',
    visibility: 'Public',
  },
  {
    id: 10,
    name: 'Do Family',
    description: 'Sporty family.',
    avatarUrl: 'https://cdn.vuetifyjs.com/images/cards/athlete.jpg',
    visibility: 'Private',
  },
]);

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
