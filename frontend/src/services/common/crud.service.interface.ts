export interface ICrudService<T> {
  fetch(): Promise<T[]>;
  getById(id: string): Promise<T | undefined>;
  add(newItem: Omit<T, 'id'>): Promise<T>;
  update(updatedItem: T): Promise<T>;
  delete(id: string): Promise<void>;
}
