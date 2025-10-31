export interface Paginated<T> {
  items: T[];
  page: number;
  totalItems: number;
  totalPages: number;
}
