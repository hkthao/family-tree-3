
import { vi } from 'vitest';

// Mock ResizeObserver
global.ResizeObserver = vi.fn(() => ({
  observe: vi.fn(),
  unobserve: vi.fn(),
  disconnect: vi.fn(),
}));// tests/setup.ts

// Mock visualViewport để Vuetify không quăng lỗi
Object.defineProperty(window, 'visualViewport', {
  value: {
    addEventListener: vi.fn(),
    removeEventListener: vi.fn(),
    width: 1024,
    height: 768,
    scale: 1,
  },
  writable: true,
});
