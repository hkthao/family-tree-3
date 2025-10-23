import { setActivePinia, createPinia } from 'pinia';
import { useChunkStore } from '@/stores/chunk.store';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import type { TextChunk } from '@/types';
import { ok, err } from '@/types';
import type { ApiError } from '@/plugins/axios';
import { createServices } from '@/services/service.factory';

// Mock the IChunkService
const mockUploadFile = vi.fn();
const mockApproveChunks = vi.fn();

// Mock the entire service factory to control service injection
vi.mock('@/services/service.factory', () => ({
  createServices: vi.fn(() => ({
    chunk: {
      uploadFile: mockUploadFile,
      approveChunks: mockApproveChunks,
    },
    // Add other services as empty objects if they are not directly used by chunk.store
    ai: {},
    auth: {},
    chat: {},
    dashboard: {},
    event: {},
    face: {},
    faceMember: {},
    family: {},
    fileUpload: {},
    member: {},
    naturalLanguageInput: {},
    notification: {},
    relationship: {},
    systemConfig: {},
    userActivity: {},
    userPreference: {},
    userProfile: {},
    userSettings: {},
  })),
}));

// Mock i18n
vi.mock('@/plugins/i18n', () => ({
  default: {
    global: {
      locale: { value: 'en' },
      t: vi.fn((key) => key),
    },
  },
}));

describe('chunk.store', () => {
  let store: ReturnType<typeof useChunkStore>;

  const mockTextChunk: TextChunk = {
    id: 'chunk-1',
    content: 'This is a test chunk.',
    metadata: {
      fileName: 'file-1.txt',
      fileId: 'file-1',
      familyId: 'family-1',
      page: '1',
      category: 'biography',
      createdBy: 'user-1',
      createdAt: '2023-01-01T00:00:00Z',
    },
    approved: false,
  };

  const mockTextChunk2: TextChunk = {
    id: 'chunk-2',
    content: 'This is another test chunk.',
    metadata: {
      fileName: 'file-1.txt',
      fileId: 'file-1',
      familyId: 'family-1',
      page: '2',
      category: 'biography',
      createdBy: 'user-1',
      createdAt: '2023-01-01T00:00:00Z',
    },
    approved: true,
  };

  beforeEach(() => {
    const pinia = createPinia();
    setActivePinia(pinia);
    store = useChunkStore();
    store.$reset();
    // Manually inject the mocked services
    // @ts-expect-error: Mocking services for testing
    store.services = createServices('mock');

    // Reset mocks before each test
    mockUploadFile.mockReset();
    mockApproveChunks.mockReset();

    // Set default mock resolved values
    mockUploadFile.mockResolvedValue(ok([mockTextChunk]));
    mockApproveChunks.mockResolvedValue(ok(undefined));
  });

  it('should have correct initial state', () => {
    expect(store.chunks).toEqual([]);
    expect(store.loading).toBe(false);
    expect(store.error).toBeNull();
  });

  describe('uploadFile', () => {
    it('should upload file successfully and set chunks', async () => {
      const file = new File(['content'], 'test.txt');
      const metadata = {
        fileId: 'file-1',
        familyId: 'family-1',
        category: 'biography',
        createdBy: 'user-1',
      };
      mockUploadFile.mockResolvedValue(ok([mockTextChunk, mockTextChunk2]));

      const result = await store.uploadFile(file, metadata);

      expect(result.ok).toBe(true);
      expect(store.loading).toBe(false);
      expect(store.error).toBeNull();
      expect(store.chunks.length).toBe(2);
      expect(store.chunks[0].approved).toBe(true);
      expect(store.chunks[1].approved).toBe(true);
      expect(mockUploadFile).toHaveBeenCalledTimes(1);
      expect(mockUploadFile).toHaveBeenCalledWith(file, metadata);
    });

    it('should handle upload file failure', async () => {
      const errorMessage = 'Upload failed.';
      mockUploadFile.mockResolvedValue(err({ message: errorMessage } as ApiError));

      const file = new File(['content'], 'test.txt');
      const metadata = {
        fileId: 'file-1',
        familyId: 'family-1',
        category: 'biography',
        createdBy: 'user-1',
      };
      const result = await store.uploadFile(file, metadata);

      expect(result.ok).toBe(false);
      expect(store.loading).toBe(false);
      expect(store.error).toBe('chunkAdmin.uploadError');
      expect(store.chunks).toEqual([]);
      expect(mockUploadFile).toHaveBeenCalledTimes(1);
    });

    it('should handle unexpected upload error', async () => {
      const errorMessage = 'Network error.';
      mockUploadFile.mockRejectedValue(new Error(errorMessage));

      const file = new File(['content'], 'test.txt');
      const metadata = {
        fileId: 'file-1',
        familyId: 'family-1',
        category: 'biography',
        createdBy: 'user-1',
      };
      const result = await store.uploadFile(file, metadata);

      expect(result.ok).toBe(false);
      expect(store.loading).toBe(false);
      expect(store.error).toBe(errorMessage);
      expect(store.chunks).toEqual([]);
      expect(mockUploadFile).toHaveBeenCalledTimes(1);
    });
  });

  describe('setChunkApproval', () => {
    it('should set chunk approval status', () => {
      store.chunks = [{ ...mockTextChunk, approved: false }];
      store.setChunkApproval('chunk-1', true);
      expect(store.chunks[0].approved).toBe(true);
    });

    it('should not change approval status if chunk not found', () => {
      store.chunks = [{ ...mockTextChunk, approved: false }];
      store.setChunkApproval('non-existent-chunk', true);
      expect(store.chunks[0].approved).toBe(false);
    });
  });

  describe('clearChunks', () => {
    it('should clear all chunks', () => {
      store.chunks = [mockTextChunk, mockTextChunk2];
      store.clearChunks();
      expect(store.chunks).toEqual([]);
    });
  });

  describe('approveChunks', () => {
    it('should approve chunks successfully and clear store chunks', async () => {
      store.chunks = [mockTextChunk, mockTextChunk2];
      const chunksToApprove = [mockTextChunk];

      const result = await store.approveChunks(chunksToApprove);

      expect(result.ok).toBe(true);
      expect(store.loading).toBe(false);
      expect(store.error).toBeNull();
      expect(store.chunks).toEqual([]); // Cleared after approval
      expect(mockApproveChunks).toHaveBeenCalledTimes(1);
      expect(mockApproveChunks).toHaveBeenCalledWith(chunksToApprove);
    });

    it('should handle approve chunks failure', async () => {
      const errorMessage = 'Approve failed.';
      mockApproveChunks.mockResolvedValue(err({ message: errorMessage } as ApiError));
      store.chunks = [mockTextChunk];
      const chunksToApprove = [mockTextChunk];

      const result = await store.approveChunks(chunksToApprove);

      expect(result.ok).toBe(false);
      expect(store.loading).toBe(false);
      expect(store.error).toBe('chunkAdmin.approveError');
      expect(store.chunks).toEqual([mockTextChunk]); // Not cleared on failure
      expect(mockApproveChunks).toHaveBeenCalledTimes(1);
    });

    it('should handle unexpected approve chunks error', async () => {
      const errorMessage = 'Network error.';
      mockApproveChunks.mockRejectedValue(new Error(errorMessage));
      store.chunks = [mockTextChunk];
      const chunksToApprove = [mockTextChunk];

      const result = await store.approveChunks(chunksToApprove);

      expect(result.ok).toBe(false);
      expect(store.loading).toBe(false);
      expect(store.error).toBe(errorMessage);
      expect(store.chunks).toEqual([mockTextChunk]); // Not cleared on failure
      expect(mockApproveChunks).toHaveBeenCalledTimes(1);
    });
  });

  describe('getters', () => {
    it('approvedChunks should return only approved chunks', () => {
      store.chunks = [
        { ...mockTextChunk, approved: true },
        { ...mockTextChunk2, approved: false },
      ];
      expect(store.approvedChunks).toEqual([{ ...mockTextChunk, approved: true }]);
    });

    it('rejectedChunks should return only rejected chunks', () => {
      store.chunks = [
        { ...mockTextChunk, approved: true },
        { ...mockTextChunk2, approved: false },
      ];
      expect(store.rejectedChunks).toEqual([{ ...mockTextChunk2, approved: false }]);
    });
  });
});