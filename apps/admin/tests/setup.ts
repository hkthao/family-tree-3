import { vi } from 'vitest';

// Mock URL.createObjectURL as it's a browser API not available in JSDOM
Object.defineProperty(global.URL, 'createObjectURL', {
  writable: true,
  value: vi.fn(() => 'mock-object-url'),
});

// Mock console.error globally to prevent test output pollution
vi.spyOn(console, 'error').mockImplementation(() => {});

// Mock ResizeObserver
const ResizeObserverMock = vi.fn(() => ({
  observe: vi.fn(),
  unobserve: vi.fn(),
  disconnect: vi.fn(),
}));

vi.stubGlobal('ResizeObserver', ResizeObserverMock);

// Mock vue-i18n globally
vi.mock('vue-i18n', () => ({
  createI18n: vi.fn(() => ({
    global: {
      t: vi.fn((key: string) => key),
    },
  })),
  useI18n: vi.fn(() => ({
    t: vi.fn((key: string) => key),
  })),
}));
