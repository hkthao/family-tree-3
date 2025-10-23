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
