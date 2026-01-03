// apps/admin/tests/unit/composables/usePagination.test.ts
import { describe, it, expect, beforeEach } from 'vitest';
import { ref, nextTick } from 'vue';
import { usePagination } from '@/composables/usePagination';

describe('usePagination', () => {
  beforeEach(() => {
    // No specific setup needed for usePagination as it's self-contained
  });

  it('should initialize with default values', () => {
    const { currentPage, itemsPerPage, totalItems, numberOfPages } = usePagination();

    expect(currentPage.value).toBe(1);
    expect(itemsPerPage.value).toBe(10);
    expect(totalItems.value).toBe(0);
    expect(numberOfPages.value).toBe(0);
  });

  it('should initialize with provided values', () => {
    const { currentPage, itemsPerPage } = usePagination(5, 20);

    expect(currentPage.value).toBe(5);
    expect(itemsPerPage.value).toBe(20);
  });

  it('should calculate numberOfPages correctly', () => {
    const { totalItems, itemsPerPage, numberOfPages } = usePagination();

    totalItems.value = 100;
    itemsPerPage.value = 10;
    expect(numberOfPages.value).toBe(10);

    totalItems.value = 99;
    expect(numberOfPages.value).toBe(10); // 99 / 10 = 9.9 => 10 pages

    totalItems.value = 101;
    expect(numberOfPages.value).toBe(11); // 101 / 10 = 10.1 => 11 pages

    itemsPerPage.value = 20;
    expect(numberOfPages.value).toBe(6); // 101 / 20 = 5.05 => 6 pages
  });

  it('should reset to first page when itemsPerPage changes', async () => {
    const { currentPage, itemsPerPage, totalItems } = usePagination(3, 10);
    totalItems.value = 100; // Set some total items for context

    expect(currentPage.value).toBe(3);

    itemsPerPage.value = 20;
    await nextTick();
    expect(currentPage.value).toBe(1);
  });

  it('should reset pagination to initial state', () => {
    const { currentPage, itemsPerPage, totalItems, resetPagination } = usePagination(2, 5);

    currentPage.value = 10;
    itemsPerPage.value = 50;
    totalItems.value = 200;

    resetPagination();

    expect(currentPage.value).toBe(2);
    expect(itemsPerPage.value).toBe(5);
    expect(totalItems.value).toBe(0);
  });
});
