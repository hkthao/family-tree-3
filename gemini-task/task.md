Create a Pinia store called "userSettingsStore" for a Vue 3 + Vuetify 3 family tree management application. 

Requirements:

1. State:
   - theme: 'light' | 'dark', default 'light'
   - notifications: object with keys { email: boolean, sms: boolean, inApp: boolean }, default { email: true, sms: false, inApp: true }
   - language: string, default 'en'

2. Actions:
   - setTheme(theme: 'light' | 'dark'): updates theme
   - toggleNotification(type: 'email' | 'sms' | 'inApp'): toggles the notification setting
   - setLanguage(lang: string): updates language
   - saveSettings(): simulate an API call to save current settings and return a success/failure message

3. Features:
   - Use Vue 3 reactive state (`ref` or `reactive`)
   - Export store using Composition API style
   - Ensure store is ready to be imported in components and bound directly to forms
   - Include comments for each section explaining purpose
