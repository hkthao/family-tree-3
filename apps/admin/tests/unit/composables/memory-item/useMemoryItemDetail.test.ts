import { describe, it, expect, vi, beforeEach } from 'vitest';
import { shallowMount } from '@vue/test-utils';
import { QueryClient, QueryClientProvider } from '@tanstack/vue-query';
import { useMemoryItemDetail } from '@/composables/memory-item/useMemoryItemDetail';
import type { MemoryItem } from '@/types';
import { ref as vueRef, type Ref } from 'vue';

// Mock the module first, returning a dummy function
// The actual spy will be created later using vi.spyOn
vi.mock('@/composables/memory-item', () => ({
  useMemoryItemQuery: vi.fn(), // Return a simple vi.fn() as the mock
}));

let mockUseMemoryItemQuerySpy: ReturnType<typeof vi.fn>;
let mockQueryReturnValue: Ref<{ data: any; isLoading: boolean; error: any }>;

const queryClient = new QueryClient();

function mountComposable(
  familyId: string = 'test-family-id',
  memoryItemId: string = 'test-memory-item-id',
  onClose = vi.fn(),
) {
  let composable: ReturnType<typeof useMemoryItemDetail>;
  shallowMount({
    setup() {
      composable = useMemoryItemDetail({ familyId, memoryItemId, onClose });
      return () => {};
    },
  }, {
    global: {
      plugins: [[QueryClientProvider, { client: queryClient }]],
    },
  });
  return composable!;
}

describe('useMemoryItemDetail', () => {
  beforeEach(async () => { // Make beforeEach async to allow await import
    vi.clearAllMocks(); // Clears all existing mocks and spies

    // Re-spy on the mocked function and set its implementation for each test
    // This ensures a clean state for each test.
    mockUseMemoryItemQuerySpy = vi.spyOn(
      (await import('@/composables/memory-item')) as any, // Cast to any to access the mocked module
      'useMemoryItemQuery',
    );
    
    mockQueryReturnValue = vueRef({ data: null, isLoading: false, error: null });
    
    mockUseMemoryItemQuerySpy.mockImplementation((_familyId: string, _memoryItemId: string) => {
      // Return the current reactive mock value
      return mockQueryReturnValue.value;
    });
  });

  it('should return memory item data, loading state, and error', () => {
    const mockMemoryItem: MemoryItem = {
      id: 'test-memory-item-id',
      familyId: 'test-family-id',
      title: 'Test Memory',
      emotionalTag: 0,
      memoryMedia: [],
      memoryPersons: [],
      personIds: [],
    };
    mockUseMemoryItemQuerySpy.mockImplementation(() => ({
      data: vueRef(mockMemoryItem),
      isLoading: vueRef(false),
      error: vueRef(null),
    }));

    const { memoryItem, isLoading, error } = mountComposable();

    expect(mockUseMemoryItemQuerySpy).toHaveBeenCalledWith('test-family-id', 'test-memory-item-id');
    expect(memoryItem.value).toEqual(mockMemoryItem);
    expect(isLoading.value).toBe(false);
    expect(error.value).toBeNull();
  });

  it('should return loading state when data is being fetched', () => {
    mockUseMemoryItemQuerySpy.mockImplementation(() => ({
      data: vueRef(null),
      isLoading: vueRef(true),
      error: vueRef(null),
    }));

    const { memoryItem, isLoading, error } = mountComposable();

    expect(mockUseMemoryItemQuerySpy).toHaveBeenCalledWith('test-family-id', 'test-memory-item-id');
    expect(memoryItem.value).toBeNull();
    expect(isLoading.value).toBe(true);
    expect(error.value).toBeNull();
  });

  it('should return error state when data fetching fails', () => {
    const mockError = new Error('Failed to fetch');
    mockUseMemoryItemQuerySpy.mockImplementation(() => ({
      data: vueRef(null),
      isLoading: vueRef(false),
      error: vueRef(mockError),
    }));

    const { memoryItem, isLoading, error } = mountComposable();

    expect(mockUseMemoryItemQuerySpy).toHaveBeenCalledWith('test-family-id', 'test-memory-item-id');
    expect(memoryItem.value).toBeNull();
    expect(isLoading.value).toBe(false);
    expect(error.value).toEqual(mockError);
  });

  it('closeView should call the onClose callback', () => {
    const mockOnClose = vi.fn();
    mockUseMemoryItemQuerySpy.mockImplementation(() => ({
      data: vueRef(null),
      isLoading: vueRef(false),
      error: vueRef(null),
    }));

    const { closeView } = mountComposable('test-family-id', 'test-memory-item-id', mockOnClose);

    closeView();

    expect(mockOnClose).toHaveBeenCalledOnce();
  });
});