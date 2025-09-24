import { fileURLToPath } from 'node:url';
import { mergeConfig, defineConfig, configDefaults } from 'vitest/config';
import viteConfig from './vite.config';

export default mergeConfig(
  viteConfig,
  defineConfig({
    test: {
      environment: 'jsdom',
      exclude: [...configDefaults.exclude, 'e2e/**'],
      root: fileURLToPath(new URL('./', import.meta.url)),
      coverage: {
        provider: 'v8',
        reporter: ['text', 'json', 'html'],
        reportsDirectory: './tests/coverage/frontend',
        all: true,
        thresholds: {
          statements: 0,
          branches: 0,
          functions: 0,
          lines: 0
        },
        include: ['src/stores/**/*.ts']
      },
      server: {
        deps: {
          inline: ['vuetify']
        }
      }
    },
  }),
);
