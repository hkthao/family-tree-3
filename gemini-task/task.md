You are a senior frontend designer and Vue 3 + Vuetify 3 developer. 
Your task is to **redesign and improve** an existing Genealogy Management Dashboard UI to look more professional, visually balanced, and data-driven.

---

## Current State
The current dashboard includes:
- Overview statistics (Families, Members, Relationships, Generations)
- Recent activity list
- Upcoming birthdays
- Family tree preview
- System info (API status)

While functional, the layout looks basic and lacks visual hierarchy, interactive charts, and modern UI accents.

---

## Design & UX Goals
Redesign the dashboard to look like a **modern admin panel** (similar to Notion / Linear / Vercel style):
- Use **card-based layout** with balanced spacing.
- Clear **section titles with icons**.
- Add **visual hierarchy** using size, color, and subtle shadows.
- Avoid overwhelming the user: minimalist, professional, data-driven.

---

## Functional Enhancements

1. **Top Summary Section (Overview Stats)**
   - Show key metrics with small icons and trend indicators (e.g. +5% vs last week).
   - Use color coding (blue for families, green for members, purple for relationships, amber for generations).
   - Add small bar sparkline or mini chart under each metric card.

2. **Middle Section**
   - Split into 2 columns:
     - **Recent Activity**: Use timeline or icon-based list.
     - **Upcoming Birthdays**: Use card with profile avatars and colored badges for age.

3. **Bottom Section**
   - **Family Tree Overview**:
     - Replace static placeholder with a mini network or D3 preview card (mock if API not ready).
   - **System Info**:
     - API status with colored chip (green/red).
     - Include “Last Sync” and “Server Time”.
     - Add small pie chart showing success vs failed requests (mock data).

4. **Charts & Visualization**
   - Use **Recharts** (or Chart.js if supported) integrated with Vuetify cards.
   - Add:
     - Bar chart for member growth over months.
     - Doughnut chart for relationship type distribution.

5. **Technical Stack**
   - Vue 3 + Vuetify 3
   - Pinia store (`dashboard.store.ts`) for fetching aggregated data
   - TypeScript interfaces for clarity
   - Mock data acceptable for now

6. **Code Style**
   - Keep component-based structure:
     - `DashboardStats.vue`
     - `DashboardCharts.vue`
     - `RecentActivity.vue`
     - `UpcomingBirthdays.vue`
     - `SystemStatus.vue`
   - Comment code for junior developers
   - Follow consistent naming and Vuetify best practices

---

## Output
Generate full Vue 3 + Vuetify 3 implementation with:
- Clean, modern layout
- At least one bar or pie chart
- Mock data for visuals
- Consistent dark theme design (matching current UI)
- All code ready to drop into `/src/views/dashboard/`

---

## Example visual tone
Think of:
- Linear.app dashboard
- GitHub Insights
- Vercel Analytics
- Tailwind UI admin panel layouts

Avoid bright neon colors or heavy gradients — keep it elegant and clear.
