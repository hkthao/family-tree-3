import { setActivePinia, createPinia } from 'pinia';
import { useFaceStore } from '@/stores/face.store';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { Gender, type DetectedFace, type SearchResult, type Member } from '@/types';
import { ok, err } from '@/types';
import type { ApiError } from '@/plugins/axios';
import { createServices } from '@/services/service.factory';

// Mock the IFaceService
const mockDetect = vi.fn();
const mockSaveLabels = vi.fn();
const mockSearch = vi.fn();
const mockDeleteFacesByMemberId = vi.fn(); // Add this mock function

// Mock the entire service factory to control service injection
vi.mock('@/services/service.factory', () => ({
  createServices: vi.fn(() => ({
    memberFace: {
      detect: mockDetect,
      saveLabels: mockSaveLabels,
      search: mockSearch,
      deleteFacesByMemberId: mockDeleteFacesByMemberId, // Add this to the mock
    },
    // Add other services as empty objects if they are not directly used by face.store
    ai: {},
    auth: {},
    chat: {},
    dashboard: {},
    event: {},
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

describe('face.store', () => {
  let store: ReturnType<typeof useFaceStore>;

  const mockDetectedFace: DetectedFace = {
    id: 'face-1',
    boundingBox: { x: 10, y: 10, width: 50, height: 50 },
    thumbnail: 'base64thumb',
    memberId: null,
    originalMemberId: null, // Add originalMemberId
    memberName: undefined,
    familyId: undefined,
    familyName: undefined,
    birthYear: undefined,
    deathYear: undefined,
    embedding: [0.1, 0.2, 0.3],
    status: 'unrecognized',
  };

  const mockMember: Member = {
    id: 'member-1',
    familyId: 'family-1',
    lastName: 'Doe',
    firstName: 'John',
    fullName: 'John Doe',
    gender: Gender.Male,
  };

  const mockFaceDetectionResult = {
    imageId: 'image-1',
    detectedFaces: [mockDetectedFace],
  };

  const mockSearchResult: SearchResult = {
    member: {
      id: 'member123',
      fullName: 'John Doe',
      avatarUrl: 'path/to/john.jpg',
      familyId: 'family-1',
      firstName: 'John',
      lastName: 'Doe',
      gender: Gender.Male,
    },
    confidence: 0.95,
  };

  beforeEach(() => {
    const pinia = createPinia();
    setActivePinia(pinia);
    store = useFaceStore();
    store.$reset();
    // Manually inject the mocked services
    store.services = createServices('test');

    // Reset mocks before each test
    (store.services.memberFace.detect as any).mockReset();
    (store.services.memberFace.saveLabels as any).mockReset();
    mockDeleteFacesByMemberId.mockReset(); // NEW
    // Set default mock resolved values on the injected service mocks
    (store.services.memberFace.detect as any).mockResolvedValue(ok(mockFaceDetectionResult));
    (store.services.memberFace.saveLabels as any).mockResolvedValue(ok(undefined));
    mockDeleteFacesByMemberId.mockResolvedValue(ok(undefined)); // NEW
  });

  it('should have correct initial state', () => {
    expect(store.uploadedImage).toBeNull();
    expect(store.uploadedImageId).toBeNull();
    expect(store.detectedFaces).toEqual([]);
    expect(store.selectedFaceId).toBeUndefined();
    expect(store.faceSearchResults).toEqual([]);
    expect(store.loading).toBe(false);
    expect(store.error).toBeNull();
  });

  describe('detectFaces', () => {
    it('should detect faces successfully', async () => {
      const file = new File(['dummy'], 'test.jpg', { type: 'image/jpeg' });
      const result = await store.detectFaces(file, false);

      expect(result.ok).toBe(true);
      expect(store.loading).toBe(false);
      expect(store.error).toBeNull();
      expect(store.uploadedImage).not.toBeNull();
      expect(store.uploadedImageId).toBe(mockFaceDetectionResult.imageId);
      expect(store.detectedFaces.length).toBe(1);
      expect(store.detectedFaces[0].id).toBe(mockDetectedFace.id);
      expect(store.detectedFaces[0].originalMemberId).toBeNull(); // New assertion
      expect(store.detectedFaces[0].status).toBe('unrecognized'); // New assertion
      expect(mockDetect).toHaveBeenCalledTimes(1);
      expect(mockDetect).toHaveBeenCalledWith(file, false);
    });

    it('should handle detect faces failure', async () => {
      const errorMessage = 'Detection failed.';
      mockDetect.mockResolvedValue(err({ message: errorMessage } as ApiError));
      const file = new File(['dummy'], 'test.jpg', { type: 'image/jpeg' });

      const result = await store.detectFaces(file, false);

      expect(result.ok).toBe(false);
      expect(store.loading).toBe(false);
      expect(store.error).toBe(errorMessage);
      expect(store.uploadedImage).toBeNull();
      expect(store.detectedFaces).toEqual([]);
      expect(mockDetect).toHaveBeenCalledTimes(1);
    });

    it('should handle unexpected error during face detection', async () => {
      const errorMessage = 'Network error.';
      mockDetect.mockRejectedValue(new Error(errorMessage));
      const file = new File(['dummy'], 'test.jpg', { type: 'image/jpeg' });

      const result = await store.detectFaces(file, false);

      expect(result.ok).toBe(false);
      expect(store.loading).toBe(false);
      expect(store.error).toBe(errorMessage);
      expect(store.uploadedImage).toBeNull();
      expect(store.detectedFaces).toEqual([]);
      expect(mockDetect).toHaveBeenCalledTimes(1);
    });
  });

  describe('selectFace', () => {
    it('should select a face', () => {
      store.detectedFaces = [mockDetectedFace];
      store.selectFace('face-1');
      expect(store.selectedFaceId).toBe('face-1');
    });

    it('should deselect face if undefined is passed', () => {
      store.selectedFaceId = 'face-1';
      store.selectFace(undefined);
      expect(store.selectedFaceId).toBeUndefined();
    });
  });

  describe('labelFace', () => {
    it('should label a face with member details', () => {
      store.detectedFaces = [{ ...mockDetectedFace }];
      store.labelFace('face-1', 'member-1', mockMember);
      expect(store.detectedFaces[0].memberId).toBe('member-1');
      expect(store.detectedFaces[0].status).toBe('labeled');
      expect(store.detectedFaces[0].memberName).toBe(mockMember.fullName);
      expect(store.detectedFaces[0].familyId).toBe(mockMember.familyId);
      expect(store.detectedFaces[0].originalMemberId).toBeNull(); // Should remain null
    });

    it('should not label if face not found', () => {
      store.detectedFaces = [{ ...mockDetectedFace }];
      store.labelFace('non-existent', 'member-1', mockMember);
      expect(store.detectedFaces[0].memberId).toBeNull();
    });
  });

  describe('removeFace', () => {
    it('should remove a face', () => {
      store.detectedFaces = [mockDetectedFace, { ...mockDetectedFace, id: 'face-2' }];
      store.removeFace('face-1');
      expect(store.detectedFaces.length).toBe(1);
      expect(store.detectedFaces[0].id).toBe('face-2');
    });

    it('should not remove if face not found', () => {
      store.detectedFaces = [mockDetectedFace];
      store.removeFace('non-existent');
      expect(store.detectedFaces.length).toBe(1);
    });
  });





  describe('resetState', () => {
    it('should reset the store state', () => {
      store.uploadedImage = 'some-image';
      store.uploadedImageId = 'some-id';
      store.detectedFaces = [mockDetectedFace];
      store.selectedFaceId = 'face-1';
      store.faceSearchResults = [mockSearchResult];
      store.loading = true;
      store.error = 'some-error';

      store.resetState();

      expect(store.uploadedImage).toBeNull();
      expect(store.uploadedImageId).toBeNull();
      expect(store.detectedFaces).toEqual([]);
      expect(store.selectedFaceId).toBeUndefined();
      expect(store.faceSearchResults).toEqual([]);
      expect(store.loading).toBe(false);
      expect(store.error).toBeNull();
    });
  });

  describe('getters', () => {
    it('currentSelectedFace should return the selected face', () => {
      store.detectedFaces = [mockDetectedFace, { ...mockDetectedFace, id: 'face-2' }];
      store.selectedFaceId = 'face-1';
      expect(store.currentSelectedFace).toEqual(mockDetectedFace);
    });

    it('currentSelectedFace should return undefined if no face selected', () => {
      store.detectedFaces = [mockDetectedFace];
      store.selectedFaceId = undefined;
      expect(store.currentSelectedFace).toBeUndefined();
    });

    it('unlabeledFaces should return faces without memberId', () => {
      store.detectedFaces = [
        { ...mockDetectedFace, memberId: null },
        { ...mockDetectedFace, id: 'face-2', memberId: 'member-2' },
      ];
      expect(store.unlabeledFaces.length).toBe(1);
      expect(store.unlabeledFaces[0].id).toBe('face-1');
    });

    it('labeledFaces should return faces with memberId', () => {
      store.detectedFaces = [
        { ...mockDetectedFace, memberId: null },
        { ...mockDetectedFace, id: 'face-2', memberId: 'member-2' },
      ];
      expect(store.labeledFaces.length).toBe(1);
      expect(store.labeledFaces[0].id).toBe('face-2');
    });
  });
});