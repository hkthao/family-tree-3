import { useState, useEffect, useCallback, useRef } from 'react';
import type { PaginatedList, ApiError } from '@/types';
import { debounce } from '@/utils/debounce'; // Assuming a debounce utility

export interface ZustandPaginatedStore<T, Q> {
  items: T[];
  loading: boolean;
  error: string | null;
  hasMore: boolean;
  page: number; // Current page in the store, 1-indexed
  fetch: (query: Q, isLoadMore: boolean) => Promise<PaginatedList<T> | null>;
  reset: () => void;
  setError: (error: string | null) => void;
}

export interface PaginatedSearchOptions<T, Q> {
  useStore: () => ZustandPaginatedStore<T, Q>; // A function that returns the Zustand store's state and actions
  initialQuery: Q; // Initial query/filter object
  debounceTime?: number; // Default to 400ms
  externalDependencies?: React.DependencyList; // Any external dependencies that should trigger a refetch (e.g., currentFamilyId)
}

export interface PaginatedSearchResult<T, Q> {
  items: T[];
  loading: boolean;
  error: string | null;
  hasMore: boolean;
  refreshing: boolean;
  searchQuery: string;
  setSearchQuery: (query: string) => void;
  filters: Q;
  setFilters: (newFilters: Partial<Q> | ((prev: Q) => Partial<Q>)) => void; // Accepts Partial<Q> or a function updater
  handleRefresh: () => void;
  handleLoadMore: () => void;
  resetAll: () => void;
}

export function usePaginatedSearch<T, Q extends { searchTerm?: string }>( // Extend Q to always have searchTerm
  options: PaginatedSearchOptions<T, Q>
): PaginatedSearchResult<T, Q> {
  const {
    useStore,
    initialQuery,
    debounceTime = 400,
    externalDependencies = [],
  } = options;

  // Destructure state and actions from the provided Zustand store hook
  const {
    items,
    loading,
    error,
    hasMore,
    fetch,
    reset,
    setError,
  } = useStore();

  const [searchQuery, setSearchQueryState] = useState(initialQuery.searchTerm || '');
  const [debouncedSearchQuery, setDebouncedSearchQuery] = useState(initialQuery.searchTerm || '');
  const [filters, setFiltersState] = useState<Q>({ ...initialQuery, searchTerm: initialQuery.searchTerm || '' } as Q);
  const [refreshing, setRefreshing] = useState(false);
  const isFetchingMoreRef = useRef(false);

  // Debounce search query
  useEffect(() => {
    const handler = setTimeout(() => {
      setDebouncedSearchQuery(searchQuery);
    }, debounceTime);

    return () => {
      clearTimeout(handler);
    };
  }, [searchQuery, debounceTime]);

  // Effect to trigger initial fetch and subsequent fetches on query/filter/dependency changes
  useEffect(() => {
    // If any external dependency changes, we should reset and refetch
    // This also handles the initial fetch
    reset(); // Reset store state (items, page, hasMore)
    const currentQuery = { ...initialQuery, ...filters, searchTerm: debouncedSearchQuery };
    fetch(currentQuery as Q, false); // Cast to Q
    // Clear error on new search/filter/dependency
    if (error) {
      setError(null);
    }
  }, [debouncedSearchQuery, filters, ...externalDependencies, fetch, reset, setError, initialQuery, error]);


  const handleRefresh = useCallback(async () => {
    if (loading) return; // Prevent multiple refresh attempts
    setRefreshing(true);
    try {
      reset(); // Reset store state before refreshing
      const currentQuery = { ...initialQuery, ...filters, searchTerm: debouncedSearchQuery };
      await fetch(currentQuery as Q, false); // Cast to Q
    } catch (e: any) {
      setError(e.message || 'Error refreshing data');
    } finally {
      setRefreshing(false);
    }
  }, [loading, reset, fetch, initialQuery, filters, debouncedSearchQuery, setError]);

  const handleLoadMore = useCallback(async () => {
    if (loading || isFetchingMoreRef.current || !hasMore || items.length === 0) return;

    isFetchingMoreRef.current = true;
    try {
      const currentQuery = { ...initialQuery, ...filters, searchTerm: debouncedSearchQuery };
      await fetch(currentQuery as Q, true); // Cast to Q
    } catch (e: any) {
      setError(e.message || 'Error loading more data');
    } finally {
      isFetchingMoreRef.current = false;
    }
  }, [loading, hasMore, items.length, fetch, initialQuery, filters, debouncedSearchQuery, setError]);

  const setSearchQuery = useCallback((query: string) => {
    setSearchQueryState(query);
    setFiltersState(prev => ({ ...prev, searchTerm: query } as Q)); // Cast to Q
  }, []);

  const setFilters = useCallback((newFilters: Partial<Q> | ((prev: Q) => Partial<Q>)) => {
    setFiltersState(prev => {
      const updatedFilters = typeof newFilters === 'function' ? newFilters(prev) : newFilters;
      return { ...prev, ...updatedFilters } as Q;
    });
  }, []);

  const resetAll = useCallback(() => {
    setSearchQueryState(initialQuery.searchTerm || '');
    setFiltersState({ ...initialQuery, searchTerm: initialQuery.searchTerm || '' } as Q); // Cast to Q
    reset(); // Reset Zustand store
  }, [initialQuery, reset]);

  return {
    items,
    loading,
    error,
    hasMore,
    refreshing,
    searchQuery,
    setSearchQuery,
    filters,
    setFilters,
    handleRefresh,
    handleLoadMore,
    resetAll,
  };
}
