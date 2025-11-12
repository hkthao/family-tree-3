// https://nuxt.com/docs/api/configuration/nuxt-config
export default defineNuxtConfig({
  compatibilityDate: '2025-07-15',
  devtools: { enabled: true },
  modules: [
    '@nuxtjs/tailwindcss',
    '@pinia/nuxt', // Pinia module
    '@nuxtjs/i18n', // i18n module
  ],
  i18n: {
    vueI18n: './i18n.config.ts', // Path to your i18n configuration
    locales: [
      { code: 'en', file: 'en.json' },
      { code: 'vi', file: 'vi.json' },
    ],
    defaultLocale: 'vi',
    langDir: '.', // Directory where your locale files are stored
  },
})
