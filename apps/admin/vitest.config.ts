import { fileURLToPath } from 'node:url';
import { mergeConfig, defineConfig, configDefaults } from 'vitest/config';
import viteConfig from './vite.config';

export default mergeConfig(
  viteConfig,
  defineConfig({
    test: {
      setupFiles: ['./tests/setup.ts'],
      environment: 'jsdom',
      include: ['tests/unit/**/*.test.ts'],
      exclude: [...configDefaults.exclude, 'e2e/**'],
      root: fileURLToPath(new URL('./', import.meta.url)),
      coverage: {
        provider: 'v8',
        reporter: ['text', 'json', 'html'],
        reportsDirectory: '../tests/coverage/frontend',
        all: true,
        thresholds: {
          statements: 0,
          branches: 0,
          functions: 0,
          lines: 0
        },
        include: ['src/stores/**/*.ts', 'src/views/**/*.vue', 'src/components/**/*.vue', 'src/composables/**/*.ts']
      },
      server: {
        deps: {
          inline: ['vuetify']
        }
      },
    },
    resolve: {
              alias: {
                '@': fileURLToPath(new URL('./src', import.meta.url)),
                '@tests': fileURLToPath(new URL('./tests/unit', import.meta.url)),
              },    },
  }),
);