import type { Result } from '@/types';

interface CacheEntry<T> {
  data: T;
  timestamp: number;
}

const DEFAULT_CACHE_TTL = 5 * 60 * 1000; // 5 minutes in milliseconds

export class IdCache<T extends { id: string }> {
  private cache = new Map<string, CacheEntry<T>>();
  private ttl: number;

  constructor(ttl: number = DEFAULT_CACHE_TTL) {
    this.ttl = ttl;
  }

  get(id: string): T | undefined {
    const entry = this.cache.get(id);
    if (entry && Date.now() - entry.timestamp < this.ttl) {
      return entry.data;
    }
    // If expired or not found, return undefined
    return undefined;
  }

  set(item: T): void {
    this.cache.set(item.id, { data: item, timestamp: Date.now() });
  }

  setMany(items: T[]): void {
    items.forEach(item => this.set(item));
  }

  delete(id: string): void {
    this.cache.delete(id);
  }

  clear(): void {
    this.cache.clear();
  }

  // Method to get multiple items by IDs, leveraging cache
  async getMany(ids: string[], fetchFn: (missingIds: string[]) => Promise<Result<T[]>>): Promise<Result<T[]>> {
    const cachedItems: T[] = [];
    const missingIds: string[] = [];

    ids.forEach(id => {
      const cached = this.get(id);
      if (cached) {
        cachedItems.push(cached);
      } else {
        missingIds.push(id);
      }
    });

    if (missingIds.length > 0) {
      const fetchResult = await fetchFn(missingIds);
      if (fetchResult.ok) {
        this.setMany(fetchResult.value);
        return { ok: true, value: [...cachedItems, ...fetchResult.value] };
      } else {
        return fetchResult; // Return error if fetching fails
      }
    }

    return { ok: true, value: cachedItems };
  }
}
