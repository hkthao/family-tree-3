import { describe, it, expect, beforeEach, vi } from 'vitest';
import { useFamilyLocationFormLogic } from '@/composables/family-location/logic/useFamilyLocationFormLogic';
import { reactive } from 'vue';
import { LocationAccuracy, LocationSource, LocationType } from '@/types';

// Mock useI18n
vi.mock('vue-i18n', () => ({
  useI18n: () => ({
    t: (key: string) => key, // Mock translation function
  }),
}));

describe('useFamilyLocationFormLogic', () => {
  const mockFamilyId = 'family-123';
  const defaultProps = {
    familyId: mockFamilyId,
  };

  beforeEach(() => {
    // Reset any state if necessary before each test
  });

  it('should initialize form with default values when no initialFamilyLocationData is provided', () => {
    const { form, locationTypeOptions, locationAccuracyOptions, locationSourceOptions } = useFamilyLocationFormLogic(defaultProps);

    expect(form.familyId).toBe(mockFamilyId);
    expect(form.name).toBe('');
    expect(form.locationType).toBe(LocationType.Other);
    expect(form.accuracy).toBe(LocationAccuracy.Estimated);
    expect(form.source).toBe(LocationSource.UserSelected);
    expect(form.id).toBe('');

    // Check if options are correctly computed (mocked t function)
    expect(locationTypeOptions.value.length).toBeGreaterThan(0);
    expect(locationAccuracyOptions.value.length).toBeGreaterThan(0);
    expect(locationSourceOptions.value.length).toBeGreaterThan(0);
  });

  it('should initialize form with provided initialFamilyLocationData', () => {
    const initialData = reactive({
      id: 'loc-1',
      familyId: mockFamilyId,
      name: 'Test Location',
      description: 'A test description',
      latitude: 10,
      longitude: 20,
      address: 'Test Address',
      locationType: LocationType.Homeland,
      accuracy: LocationAccuracy.Exact,
      source: LocationSource.Geocoded,
      created: new Date().toISOString(),
      createdBy: 'user-a',
      lastModified: new Date().toISOString(),
      lastModifiedBy: 'user-a',
    });

    const { form } = useFamilyLocationFormLogic({ ...defaultProps, initialFamilyLocationData: initialData });

    expect(form.id).toBe('loc-1');
    expect(form.name).toBe('Test Location');
    expect(form.locationType).toBe(LocationType.Homeland);
  });

  it('should update form when initialFamilyLocationData prop changes', async () => {
    const initialData1 = reactive({
      id: 'loc-1',
      familyId: mockFamilyId,
      name: 'Location One',
      description: 'Desc One',
      latitude: 1,
      longitude: 1,
      address: 'Address One',
      locationType: LocationType.Other,
      accuracy: LocationAccuracy.Estimated,
      source: LocationSource.UserSelected,
      created: new Date().toISOString(),
      createdBy: 'user-a',
      lastModified: new Date().toISOString(),
      lastModifiedBy: 'user-a',
    });

    const initialData2 = reactive({
      id: 'loc-2',
      familyId: mockFamilyId,
      name: 'Location Two',
      description: 'Desc Two',
      latitude: 2,
      longitude: 2,
      address: 'Address Two',
      locationType: LocationType.Grave,
      accuracy: LocationAccuracy.Exact,
      source: LocationSource.Geocoded,
      created: new Date().toISOString(),
      createdBy: 'user-b',
      lastModified: new Date().toISOString(),
      lastModifiedBy: 'user-b',
    });

    const propsWithReactiveData = reactive({ ...defaultProps, initialFamilyLocationData: initialData1 });
    const { form } = useFamilyLocationFormLogic(propsWithReactiveData);

    expect(form.id).toBe('loc-1');
    expect(form.name).toBe('Location One');

    // Simulate prop change
    propsWithReactiveData.initialFamilyLocationData = initialData2;

    // Await next tick to allow watcher to run (although not strictly necessary for reactive data in test environment)
    // await nextTick(); // nextTick is not directly needed for reactive property changes in vitest/vue test utils

    expect(form.id).toBe('loc-2');
    expect(form.name).toBe('Location Two');
  });

  it('should reset form when initialFamilyLocationData prop becomes null', () => {
    const initialData = reactive({
      id: 'loc-1',
      familyId: mockFamilyId,
      name: 'Test Location',
      description: 'A test description',
      latitude: 10,
      longitude: 20,
      address: 'Test Address',
      locationType: LocationType.Homeland,
      accuracy: LocationAccuracy.Exact,
      source: LocationSource.Geocoded,
      created: new Date().toISOString(),
      createdBy: 'user-a',
      lastModified: new Date().toISOString(),
      lastModifiedBy: 'user-a',
    });

    const propsWithReactiveData = reactive({ ...defaultProps, initialFamilyLocationData: initialData });
    const { form } = useFamilyLocationFormLogic(propsWithReactiveData);

    expect(form.name).toBe('Test Location');

    propsWithReactiveData.initialFamilyLocationData = undefined; // Simulate becoming null/undefined

    expect(form.name).toBe('');
    expect(form.id).toBe(''); // Check that ID is reset
    expect(form.familyId).toBe(mockFamilyId);
  });
});
