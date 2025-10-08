import type { IRelationshipService } from './relationship.service.interface';
import type { Relationship, Paginated, Result } from '@/types';
import { ok, err } from '@/types';

export class MockRelationshipService implements IRelationshipService {
  private relationships: Relationship[] = [];

  async fetch(): Promise<Result<Relationship[], any>> {
    return ok(this.relationships);
  }

  async getById(id: string): Promise<Result<Relationship | undefined, any>> {
    return ok(this.relationships.find(r => r.id === id));
  }

  async add(newItem: Omit<Relationship, 'id'>): Promise<Result<Relationship, any>> {
    const newRelationship = { ...newItem, id: Math.random().toString() };
    this.relationships.push(newRelationship);
    return ok(newRelationship);
  }

  async update(updatedItem: Relationship): Promise<Result<Relationship, any>> {
    const index = this.relationships.findIndex(r => r.id === updatedItem.id);
    if (index !== -1) {
      this.relationships[index] = updatedItem;
      return ok(updatedItem);
    }
    return err({ message: 'Relationship not found', statusCode: 404 });
  }

  async delete(id: string): Promise<Result<void, any>> {
    this.relationships = this.relationships.filter(r => r.id !== id);
    return ok(undefined);
  }

  async loadItems(): Promise<Result<Paginated<Relationship>, any>> {
    const paginated: Paginated<Relationship> = {
      items: this.relationships,
      totalItems: this.relationships.length,
      totalPages: 1,
    };
    return ok(paginated);
  }

  async getByIds(ids: string[]): Promise<Result<Relationship[], any>> {
    return ok(this.relationships.filter(r => ids.includes(r.id)));
  }
}
