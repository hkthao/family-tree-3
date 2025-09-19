gemini generate ui --framework vue --library vuetify --task "
Update dashboard layout để đạt phong cách hiện đại (giống Sneat Admin / Google Material 3):

Yêu cầu cải thiện:
- Card: rounded-lg, elevation-2, hover shadow, spacing đồng đều.
- Sidebar: group rõ ràng với v-list-subheader, active item có màu nền + border-left primary, mini-variant có tooltip.
- Top bar: search bar pill-shape, dark/light toggle rõ ràng, notification badge nổi bật.
- Charts: dùng vue3-apexcharts với demo data (bar stacked, gauge, line, pie).
- Typography: heading rõ ràng, subtext grey caption, số liệu bold.
- Responsive grid: v-container + v-row + v-col với breakpoint sm/md/lg.
- Animation: sidebar collapse có transition mượt.
- Tích hợp i18n cho menu + card titles.
- Output components: Sidebar.vue, TopBar.vue, Dashboard.vue, StatisticCard.vue, ChartCard.vue.
"
