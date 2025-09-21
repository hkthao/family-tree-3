import { setActivePinia, createPinia } from 'pinia';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { useFamiliesStore } from '../families';
import { MockFamilyService, type Family } from '../../services/family.service';

describe('useFamiliesStore', () => {
  let mockService: MockFamilyService;
  let initialFamilies: Family[];

  beforeEach(() => {
    setActivePinia(createPinia());
    vi.resetAllMocks();

    initialFamilies = [
      { id: '1', name: 'Family A', description: 'Desc A', address: 'Addr A', visibility: 'Public', createdAt: new Date().toISOString(), updatedAt: new Date().toISOString() },
      { id: '2', name: 'Family B', description: 'Desc B', address: 'Addr B', visibility: 'Private', createdAt: new Date().toISOString(), updatedAt: new Date().toISOString() },
      { id: '3', name: 'Family C', description: 'Desc C', address: 'Addr C', visibility: 'Public', createdAt: new Date().toISOString(), updatedAt: new Date().toISOString() },
    ];
    mockService = new MockFamilyService(initialFamilies);
  });

  it('should fetch all families', async () => {
    const store = useFamiliesStore(mockService);
    await store.fetchAll();

    expect(store.families.length).toBe(initialFamilies.length);
    expect(store.total).toBe(initialFamilies.length);
    expect(store.loading).toBe(false);
    expect(mockService.fetchFamilies).toHaveBeenCalledTimes(1);
  });

  it('should add a family', async () => {
    const store = useFamiliesStore(mockService);
    const newFamilyData = {
      name: 'New Family',
      description: 'New Desc',
      address: 'New Addr',
      visibility: 'Public',
    };

    await store.add(newFamilyData);

    expect(store.families.length).toBe(initialFamilies.length + 1);
    expect(store.total).toBe(initialFamilies.length + 1);
    expect(store.families.some(f => f.name === newFamilyData.name)).toBe(true);
    expect(store.loading).toBe(false);
    expect(mockService.addFamily).toHaveBeenCalledTimes(1);
    expect(mockService.addFamily).toHaveBeenCalledWith(newFamilyData);
  });

  it('should update a family', async () => {
    const store = useFamiliesStore(mockService);
    const familyToUpdate = initialFamilies[0];
    const updatedName = 'Updated Family Name';

    await store.update(familyToUpdate.id, { name: updatedName });

    expect(store.families.find(f => f.id === familyToUpdate.id)?.name).toBe(updatedName);
    expect(store.loading).toBe(false);
    expect(mockService.updateFamily).toHaveBeenCalledTimes(1);
    expect(mockService.updateFamily).toHaveBeenCalledWith(familyToUpdate.id, { name: updatedName });
  });

  it('should delete a family', async () => {
    const store = useFamiliesStore(mockService);
    const familyToDelete = initialFamilies[0];

    await store.remove(familyToDelete.id);

    expect(store.families.length).toBe(initialFamilies.length - 1);
    expect(store.total).toBe(initialFamilies.length - 1);
    expect(store.families.some(f => f.id === familyToDelete.id)).toBe(false);
    expect(store.loading).toBe(false);
    expect(mockService.removeFamily).toHaveBeenCalledTimes(1);
    expect(mockService.removeFamily).toHaveBeenCalledWith(familyToDelete.id);
  });

  it('should fetch a single family by id', async () => {
    const store = useFamiliesStore(mockService);
    const familyId = initialFamilies[0].id;
    const family = await store.fetchOne(familyId);

    expect(family).toEqual(initialFamilies[0]);
    expect(store.loading).toBe(false);
    expect(mockService.fetchFamilyById).toHaveBeenCalledTimes(1);
    expect(mockService.fetchFamilyById).toHaveBeenCalledWith(familyId);
  });

  it('should handle fetchAll error', async () => {
    const store = useFamiliesStore(mockService);
    vi.spyOn(mockService, 'fetchFamilies').mockRejectedValueOnce(new Error('Fetch error'));
    vi.spyOn(console, 'error').mockImplementation(() => {}); // Suppress console error

    await store.fetchAll();

    expect(store.error).toBe('Fetch error');
    expect(store.loading).toBe(false);
  });

  it('should handle add error', async () => {
    const store = useFamiliesStore(mockService);
    vi.spyOn(mockService, 'addFamily').mockRejectedValueOnce(new Error('Add error'));
    vi.spyOn(console, 'error').mockImplementation(() => {}); // Suppress console error

    const newFamilyData = {
      name: 'New Family',
      description: 'New Desc',
      address: 'New Addr',
      visibility: 'Public',
    };

    await expect(store.add(newFamilyData)).rejects.toThrow('Add error');
    expect(store.error).toBe('Add error');
    expect(store.loading).toBe(false);
  });

  it('should handle update error', async () => {
    const store = useFamiliesStore(mockService);
    vi.spyOn(mockService, 'updateFamily').mockRejectedValueOnce(new Error('Update error'));
    vi.spyOn(console, 'error').mockImplementation(() => {}); // Suppress console error

    const familyToUpdate = initialFamilies[0];
    const updatedName = 'Updated Family Name';

    await expect(store.update(familyToUpdate.id, { name: updatedName })).rejects.toThrow('Update error');
    expect(store.error).toBe('Update error');
    expect(store.loading).toBe(false);
  });

  it('should handle remove error', async () => {
    const store = useFamiliesStore(mockService);
    vi.spyOn(mockService, 'removeFamily').mockRejectedValueOnce(new Error('Remove error'));
    vi.spyOn(console, 'error').mockImplementation(() => {}); // Suppress console error

    const familyToDelete = initialFamilies[0];

    await expect(store.remove(familyToDelete.id)).rejects.toThrow('Remove error');
    expect(store.error).toBe('Remove error');
    expect(store.loading).toBe(false);
  });

  it('should handle fetchOne error', async () => {
    const store = useFamiliesStore(mockService);
    vi.spyOn(mockService, 'fetchFamilyById').mockRejectedValueOnce(new Error('Fetch One error'));
    vi.spyOn(console, 'error').mockImplementation(() => {}); // Suppress console error

    const familyId = initialFamilies[0].id;

    const result = await store.fetchOne(familyId);

    expect(result).toBeUndefined();
    expect(store.error).toBe('Fetch One error');
    expect(store.loading).toBe(false);
  });
});
