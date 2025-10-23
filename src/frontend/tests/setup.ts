import { vi } from 'vitest';

// Mock URL.createObjectURL as it's a browser API not available in JSDOM
Object.defineProperty(global.URL, 'createObjectURL', {
  writable: true,
  value: vi.fn(() => 'mock-object-url'),
});