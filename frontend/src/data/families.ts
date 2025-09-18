
import { ref } from 'vue';

export interface Family {
  id: number;
  Name: string;
  Description?: string;
  AvatarUrl?: string;
  Visibility: 'Private' | 'Public';
}

const families = ref<Family[]>([
  {
    id: 1,
    Name: 'Huynh Family',
    Description: 'A large and loving family.',
    AvatarUrl: 'https://cdn.vuetifyjs.com/images/cards/halcyon.png',
    Visibility: 'Public',
  },
  {
    id: 2,
    Name: 'Nguyen Family',
    Description: 'Close-knit family with strong traditions.',
    AvatarUrl: 'https://cdn.vuetifyjs.com/images/cards/sunshine.jpg',
    Visibility: 'Private',
  },
  {
    id: 3,
    Name: 'Tran Family',
    Description: 'Modern family with diverse interests.',
    AvatarUrl: 'https://cdn.vuetifyjs.com/images/cards/docks.jpg',
    Visibility: 'Public',
  },
  {
    id: 4,
    Name: 'Le Family',
    Description: 'Small family, big dreams.',
    AvatarUrl: 'https://cdn.vuetifyjs.com/images/cards/road.jpg',
    Visibility: 'Private',
  },
  {
    id: 5,
    Name: 'Pham Family',
    Description: 'Family of adventurers.',
    AvatarUrl: 'https://cdn.vuetifyjs.com/images/cards/forest.jpg',
    Visibility: 'Public',
  },
  {
    id: 6,
    Name: 'Vo Family',
    Description: 'Artistic and creative family.',
    AvatarUrl: 'https://cdn.vuetifyjs.com/images/cards/plane.jpg',
    Visibility: 'Private',
  },
  {
    id: 7,
    Name: 'Hoang Family',
    Description: 'Family of scholars.',
    AvatarUrl: 'https://cdn.vuetifyjs.com/images/cards/house.jpg',
    Visibility: 'Public',
  },
  {
    id: 8,
    Name: 'Dang Family',
    Description: 'Musical family.',
    AvatarUrl: 'https://cdn.vuetifyjs.com/images/cards/server-room.jpg',
    Visibility: 'Private',
  },
  {
    id: 9,
    Name: 'Bui Family',
    Description: 'Family of food lovers.',
    AvatarUrl: 'https://cdn.vuetifyjs.com/images/cards/cooking.png',
    Visibility: 'Public',
  },
  {
    id: 10,
    Name: 'Do Family',
    Description: 'Sporty family.',
    AvatarUrl: 'https://cdn.vuetifyjs.com/images/cards/athlete.jpg',
    Visibility: 'Private',
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
            f.Name.toLowerCase().includes(search.toLowerCase())
          );
        }

        if (visibility !== 'All') {
          filteredFamilies = filteredFamilies.filter(
            (f) => f.Visibility === visibility
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
