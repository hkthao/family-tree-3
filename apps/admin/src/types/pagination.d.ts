export interface Paginated<T> {
  items: T[];
  page: number;
  totalItems: number;
  totalPages: number;
}

export interface ListOptions {
  page?: number;
  itemsPerPage?: number;
  sortBy?: { key: string; order: 'asc' | 'desc' }[];
}

export interface FilterOptions {
  [key: string]: any; // Allows for any key-value pair for filters
}
