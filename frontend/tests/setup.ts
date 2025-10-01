
import { vi } from 'vitest';

// Polyfill for visualViewport for jsdom environment
if (typeof global.visualViewport === 'undefined') {
  global.visualViewport = {
    width: 1024,
    height: 768,
    // Add other properties as needed by Vuetify or other libraries
    // For example, addEventListener and removeEventListener if they are called
    addEventListener: vi.fn(),
    removeEventListener: vi.fn(),
  } as any;
}

// Polyfill for ResizeObserver for jsdom environment
if (typeof global.ResizeObserver === 'undefined') {
  global.ResizeObserver = vi.fn(() => ({
    observe: vi.fn(),
    unobserve: vi.fn(),
    disconnect: vi.fn(),
  }));
}
