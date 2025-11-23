<template>
  <div class="family-tree-pdf-template">
    <!-- Cover Page -->
    <section class="cover-page">
      <h1>Cây Gia Phả Họ {{ family.name }}</h1>
      <p>Ngày xuất: {{ exportDate }}</p>
      <img src="https://via.placeholder.com/300x200?text=Cover+Image" alt="Cover Image" class="cover-image" />
    </section>

    <!-- Ancestor Photos (Placeholder) -->
    <section class="ancestor-photos">
      <h2>Ảnh Tổ Tiên</h2>
      <div class="photo-grid">
        <img v-for="i in 4" :key="i" :src="`https://via.placeholder.com/150?text=Ancestor+${i}`" alt="Ancestor Photo" class="ancestor-photo" />
      </div>
    </section>

    <!-- Family Tree Visualization (Placeholder) -->
    <section class="family-tree-visualization">
      <h2>Cây Gia Phả</h2>
      <div class="tree-placeholder">
        <p>Biểu đồ cây gia phả sẽ được hiển thị ở đây.</p>
        <img src="https://via.placeholder.com/600x400?text=Family+Tree+Diagram" alt="Family Tree Diagram" />
      </div>
    </section>

    <!-- Family Members List -->
    <section class="family-members">
      <h2>Danh Sách Thành Viên</h2>
      <table>
        <thead>
          <tr>
            <th>Họ và Tên</th>
            <th>Giới Tính</th>
            <th>Ngày Sinh</th>
            <th>Sự Kiện</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="member in family.members" :key="member.id">
            <td>{{ member.fullName }}</td>
            <td>{{ member.gender }}</td>
            <td>{{ member.dateOfBirth ? member.dateOfBirth : 'N/A' }}</td>
            <td>{{ member.eventMembersCount }}</td>
          </tr>
        </tbody>
      </table>
    </section>

    <!-- Timeline (Placeholder) -->
    <section class="timeline">
      <h2>Dòng Thời Gian Các Sự Kiện</h2>
      <p>Dòng thời gian các sự kiện quan trọng trong gia đình sẽ được hiển thị ở đây.</p>
      <img src="https://via.placeholder.com/600x200?text=Timeline" alt="Timeline Placeholder" />
    </section>
  </div>
</template>

<script setup lang="ts">
import { defineProps, computed } from 'vue';
import type { FamilyPdfExportData } from '@/types/pdf'; // Import the new PDF DTO

const props = defineProps<{
  family: FamilyPdfExportData; // Use the dedicated PDF DTO
}>();

const exportDate = computed(() => {
  return new Date().toLocaleDateString('vi-VN');
});
</script>

<style scoped>
/* Base styles for the entire PDF template */
.family-tree-pdf-template {
  font-family: 'Times New Roman', serif;
  color: #333;
  margin: 20mm; /* Standard print margin */
  line-height: 1.6;
  font-size: 12pt;
}

h1, h2 {
  font-family: 'Times New Roman', serif;
  color: #000;
  margin-top: 1em;
  margin-bottom: 0.5em;
}

h1 {
  font-size: 24pt;
  text-align: center;
}

h2 {
  font-size: 18pt;
  border-bottom: 1px solid #ccc;
  padding-bottom: 5px;
  margin-top: 20pt;
}

section {
  page-break-after: always; /* Each section starts on a new page */
  padding-bottom: 10mm; /* Space at the bottom of each section */
}

/* Cover Page Styles */
.cover-page {
  text-align: center;
  padding-top: 50mm;
}

.cover-page h1 {
  margin-bottom: 20mm;
}

.cover-page p {
  font-size: 14pt;
  margin-bottom: 30mm;
}

.cover-image {
  max-width: 80%;
  height: auto;
  border: 1px solid #ddd;
}

/* Ancestor Photos Styles */
.ancestor-photos .photo-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(150px, 1fr));
  gap: 15px;
  justify-items: center;
  align-items: center;
  margin-top: 15px;
}

.ancestor-photo {
  max-width: 100%;
  height: auto;
  border: 1px solid #ddd;
  box-shadow: 2px 2px 5px rgba(0,0,0,0.2);
}

/* Family Tree Visualization Styles */
.family-tree-visualization .tree-placeholder {
  text-align: center;
  margin-top: 20px;
}
.family-tree-visualization img {
  max-width: 100%;
  height: auto;
  border: 1px solid #ddd;
}

/* Family Members Table Styles */
.family-members table {
  width: 100%;
  border-collapse: collapse;
  margin-top: 15px;
}

.family-members th, .family-members td {
  border: 1px solid #ccc;
  padding: 8px 12px;
  text-align: left;
}

.family-members th {
  background-color: #f2f2f2;
  font-weight: bold;
}

/* Timeline Styles */
.timeline {
  text-align: center;
  margin-top: 20px;
}
.timeline img {
  max-width: 100%;
  height: auto;
  border: 1px solid #ddd;
}

/* Ensure the last page does not have a page-break-after if it's the very last content */
.family-tree-pdf-template section:last-child {
    page-break-after: auto;
}
</style>