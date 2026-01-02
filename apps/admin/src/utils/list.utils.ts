import { type ListOptions, type FilterOptions } from '@/types';

export function buildSearchParams(
  options: ListOptions = { page: 1, itemsPerPage: 10, sortBy: [] },
  filters: FilterOptions = {},
): Record<string, any> {
  const params: Record<string, any> = {
    page: options.page,
    itemsPerPage: options.itemsPerPage,
  };
  if (options.sortBy && options.sortBy.length > 0) {
    params.sortBy = options.sortBy[0].key;
    params.sortOrder = options.sortBy[0].order;
  }

  for (const key in filters) {
    if (filters[key] !== undefined) {
      params[key] = filters[key];
    }
  }
  return params;
}
