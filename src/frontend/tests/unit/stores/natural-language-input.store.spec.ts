import { setActivePinia, createPinia } from 'pinia';
import { useNaturalLanguageStore } from '@/stores/naturalLanguage.store';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { createServices } from '@/services/service.factory';

// Mock the INaturalLanguageInputService
const mockGenerateFamilyData = vi.fn();
const mockGenerateMemberData = vi.fn();
const mockGenerateEventData = vi.fn();
const mockGenerateRelationshipData = vi.fn();

// Mock the entire service factory to control service injection
vi.mock('@/services/service.factory', () => ({
  createServices: vi.fn(() => ({
    naturalLanguageInput: {
      generateFamilyData: mockGenerateFamilyData,
      generateMemberData: mockGenerateMemberData,
      generateEventData: mockGenerateEventData,
      generateRelationshipData: mockGenerateRelationshipData,
    },
    // Add other services as empty objects if they are not directly used by natural-language-input.store
    ai: {},
    auth: {},
    chat: {},
    dashboard: {},
    event: {},
    face: {},
    family: {},
    fileUpload: {},
    member: {},
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
      t: vi.fn((key) => key),
    },
  },
}));

describe('natural-language-input.store', () => {
  let store: ReturnType<typeof useNaturalLanguageStore>;

  beforeEach(() => {
    vi.clearAllMocks();
    setActivePinia(createPinia());
    store = useNaturalLanguageStore();
    store.$reset();
    // Inject the mocked services
    store.services = createServices('test');

    // Reset mocks before each test
    mockGenerateFamilyData.mockReset();
    mockGenerateMemberData.mockReset();
    mockGenerateEventData.mockReset();
    mockGenerateRelationshipData.mockReset();
  });

  it('should have correct initial state', () => {
    expect(store.loading).toBe(false);
    expect(store.error).toBeNull();
  });
});