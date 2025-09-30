Create a Vue 3 + Vuetify page called "UserSettingsPage" for a family tree management application. 
Requirements:

1. Layout:
   - Responsive design with a sidebar navigation for "Profile", "Preferences", "Security".
   - Main content area changes according to selected tab.

2. Profile Tab:
   - Form with fields: Full Name, Email, Profile Picture upload.
   - "Save" button with form validation.

3. Preferences Tab:
   - Theme selection (Light / Dark).
   - Notification preferences (checkboxes: Email, SMS, In-app).
   - "Save" button.

4. Security Tab:
   - Change password form: Current Password, New Password, Confirm Password.
   - "Save" button with validation: passwords match, minimum length 8.

5. General:
   - Use Vuetify components only.
   - Provide reactive state with Vue 3 `ref` or `reactive`.
   - Include basic validation feedback (required fields, password mismatch).
   - Keep design clean and simple, similar to Vuetify style guide.
   - Use only images/icons available in Vuetify (no custom assets needed).

6. Bonus:
   - Include a snackbar notification after saving changes.
   - Structure code with components per tab if possible.
