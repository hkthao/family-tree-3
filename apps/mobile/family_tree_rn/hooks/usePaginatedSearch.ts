import { useState, useEffect, useCallback, useRef, useMemo } from 'react';
import type { PaginatedList, ApiError } from '@/types';
import { debounce } from '@/utils/debounce'; // Assuming a debounce utility

// Helper for shallow comparison
const shallowEqual = (objA: any, objB: any) => {
  if (objA === objB) {
    return true;
  }
  if (typeof objA !== 'object' || objA === null || typeof objB !== 'object' || objB === null) {
    return false;
  }
  const keysA = Object.keys(objA);
  const keysB = Object.keys(objB);
  if (keysA.length !== keysB.length) {
    return false;
  }
  for (let i = 0; i < keysA.length; i++) {
    if (!Object.prototype.hasOwnProperty.call(objB, keysA[i]) || objA[keysA[i]] !== objB[keysA[i]]) {
      return false;
    }
  }
  return true;
};

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

export function usePaginatedSearch<T, Q extends { searchTerm?: string; page?: number; itemsPerPage?: number }>( // Extend Q to always have searchTerm
  options: PaginatedSearchOptions<T, Q>
): PaginatedSearchResult<T, Q> {
  const {
    useStore,
    initialQuery: rawInitialQuery, // Renamed to differentiate from the memoized version
    debounceTime = 400,
    externalDependencies: rawExternalDependencies = [],
  } = options;

  // Stabilize initialQuery: only update its ref if the content changes
  const stableInitialQueryRef = useRef(rawInitialQuery);
  if (!shallowEqual(stableInitialQueryRef.current, rawInitialQuery)) {
    stableInitialQueryRef.current = rawInitialQuery;
  }
  const initialQuery = stableInitialQueryRef.current;

  // Stabilize externalDependencies: only update its ref if the content changes
  const stableExternalDependenciesRef = useRef(rawExternalDependencies);
  if (!shallowEqual(stableExternalDependenciesRef.current, rawExternalDependencies)) {
    stableExternalDependenciesRef.current = rawExternalDependencies;
  }
  const externalDependencies = stableExternalDependenciesRef.current;

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

  // Memoize the current query object to use as a dependency for the effect
  const currentQuery = useMemo(() => ({
    ...initialQuery, // Now using the stabilized initialQuery
    ...filters,
    searchTerm: debouncedSearchQuery
  }), [
    initialQuery, // Stabilized initialQuery is now a dependency
    debouncedSearchQuery,
    filters?.page,       // Explicitly using primitive properties from filters with optional chaining.
    filters?.itemsPerPage, // Assuming 'page' and 'itemsPerPage' are primitive properties of Q.
    // If 'Q' contains other primitive properties, they should be added here
    // to ensure 'currentQuery' is re-memoized only when these primitives change.
  ]);

  // Use a ref to track the previous query to prevent re-fetching if only reference changes
  const previousFetchedQueryRef = useRef<Q | null>(null);

  // Debounce search query
  useEffect(() => {
    console.log('Setting debounce for search query:', searchQuery);
    const handler = setTimeout(() => {
      setDebouncedSearchQuery(searchQuery);
    }, debounceTime);

    return () => {
      clearTimeout(handler);
    };
  }, [searchQuery, debounceTime]);

  // Effect to trigger initial fetch and subsequent fetches on query/filter/dependency changes
  useEffect(() => {
    // Only fetch if the query content has actually changed OR it's the very first fetch (previousFetchedQueryRef.current is null)
    if (previousFetchedQueryRef.current && shallowEqual(previousFetchedQueryRef.current, currentQuery)) {
      console.log('Current query is shallowly equal to previous, skipping fetch.');
      return;
    }

    // When a new fetch is initiated, clear any previous error
    if (error) { // Check if there was an error from a previous operation
      setError(null);
    }

    fetch(currentQuery as Q, false);

    previousFetchedQueryRef.current = currentQuery;
  }, [currentQuery, fetch, setError, ...externalDependencies]);

  const handleRefresh = useCallback(async () => {
    if (loading) return;
    setRefreshing(true);
    try {
      reset();
      await fetch(currentQuery as Q, false);
    } catch (e: any) {
      setError(e.message || 'Error refreshing data');
    } finally {
      setRefreshing(false);
    }
  }, [loading, reset, fetch, currentQuery, setError]);

  const handleLoadMore = useCallback(async () => {
    if (loading || isFetchingMoreRef.current || !hasMore || items.length === 0) return;

    isFetchingMoreRef.current = true;
    try {
      await fetch(currentQuery as Q, true);
    } catch (e: any) {
      setError(e.message || 'Error loading more data');
    } finally {
      isFetchingMoreRef.current = false;
    }
  }, [loading, hasMore, items.length, fetch, currentQuery, setError]);

  const setSearchQuery = useCallback((query: string) => {
    setSearchQueryState(query);
    setFiltersState(prev => {
      const newFilters = { ...prev, searchTerm: query } as Q;
      if (!shallowEqual(prev, newFilters)) {
        return newFilters;
      }
      return prev;
    });
  }, []);

  const setFilters = useCallback((newFilters: Partial<Q> | ((prev: Q) => Partial<Q>)) => {
    setFiltersState(prev => {
      const updatedFilters = typeof newFilters === 'function' ? newFilters(prev) : newFilters;
      const finalFilters = { ...prev, ...updatedFilters } as Q;
      if (!shallowEqual(prev, finalFilters)) {
        return finalFilters;
      }
      return prev;
    });
  }, []);

  const resetAll = useCallback(() => {
    setSearchQueryState(initialQuery.searchTerm || '');
    // Ensure initialQuery is stable or memoized by the consumer to avoid re-renders here
    const resetFilters = { ...initialQuery, searchTerm: initialQuery.searchTerm || '' } as Q;
    setFiltersState(prev => {
      if (!shallowEqual(prev, resetFilters)) {
        return resetFilters;
      }
      return prev;
    });
    reset();
  }, [initialQuery, reset]); // initialQuery is a dependency here for the resetFilters object creation.

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