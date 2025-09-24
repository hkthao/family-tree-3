export interface Paginated<T> {
  items: T[];
  totalItems: number;
  totalPages: number;
}
