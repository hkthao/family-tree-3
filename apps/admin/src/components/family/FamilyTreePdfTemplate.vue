<template>
  <div class="family-tree-pdf-template">
    <!-- Cover Page -->
    <section class="cover-page page-break-after">
      <div class="header-text">KỶ YẾU DÒNG TỘC</div>
      <h1 class="main-title">{{ family.name?.toUpperCase() }} THẾ GIA</h1>

      <div class="central-emblem">
        <!-- Placeholder for Ancestor Photo or Family Emblem -->
        <img src="https://via.placeholder.com/250x250?text=Emblem" alt="Family Emblem" class="emblem-image" />
        <div class="nomen-han-script">Phúc</div> <!-- Placeholder for Han Nom character -->
      </div>

      <p class="subtitle">Cội nguồn & Di sản</p>
      <div class="footer-text">Phụng lập năm {{ currentYearLunar }} - {{ currentYear }}</div>
    </section>

    <!-- Foreword / Introduction Page -->
    <section class="foreword-page page-break-after">
      <div class="decorative-divider"></div>
      <h2>Lời Nói Đầu</h2>
      <div class="foreword-content">
        <p>
          Trong dòng chảy lịch sử đầy biến động của dân tộc Việt Nam, mỗi dòng họ là một mạch nguồn, góp phần tạo nên dòng sông văn hóa cội nguồn. Cuốn gia phả này, mang tên "Cội Nguồn Vĩnh Cửu", được biên soạn với tất cả lòng thành kính và biết ơn vô hạn đối với các bậc tiền nhân, những người đã khai mở, gây dựng và bảo tồn dòng họ chúng ta qua bao thế hệ.
        </p>
        <p>
          Lịch sử dòng họ không chỉ là những cái tên, ngày tháng, mà còn là chuỗi dài những câu chuyện về đức hy sinh, lòng kiên trung, ý chí vượt khó và những đóng góp thầm lặng nhưng vĩ đại của tổ tiên. Từng trang giấy, từng dòng chữ trong cuốn gia phả này đều chứa đựng tâm huyết, là sự chắt lọc từ những ký ức, sử liệu và truyền khẩu, nhằm tái hiện một cách chân thực nhất hành trình mà dòng họ đã đi qua.
        </p>
        <p>
          Mục đích cao cả của việc biên soạn gia phả không chỉ dừng lại ở việc truy nguyên nguồn gốc, ghi chép thế thứ, mà còn là để giáo dục con cháu về đạo lý "uống nước nhớ nguồn", về tình thân, về trách nhiệm kế thừa và phát huy truyền thống tốt đẹp của cha ông. Mỗi thành viên khi đọc cuốn gia phả này, hy vọng sẽ tìm thấy một phần hồn cốt của mình trong dòng chảy lịch sử của dòng họ, để từ đó thêm yêu quý, tự hào và có ý thức giữ gìn, vun đắp cho thế hệ mai sau.
        </p>
        <p>
          Chúng tôi hiểu rằng, trong quá trình sưu tầm, biên soạn, dù đã cố gắng hết sức, song không thể tránh khỏi những thiếu sót hoặc sai lệch. Kính mong các bậc cao niên, quý chú bác, cô dì cùng toàn thể con cháu trong và ngoài nước lượng thứ, góp ý để cuốn gia phả ngày càng hoàn thiện hơn, xứng đáng là báu vật tinh thần của dòng họ.
        </p>
        <p>
          Nguyện cầu hồng ân của tổ tiên luôn soi sáng, phù hộ cho toàn thể con cháu trong dòng họ {{ family.name }} luôn được bình an, hạnh phúc, thành đạt và phát triển hưng thịnh, đời đời vĩnh cửu.
        </p>
      </div>
      <div class="author-info">
        <p>Ban biên soạn Gia Phả Họ {{ family.name }}</p>
        <p>{{ exportDate }}</p>
      </div>
    </section>

    <!-- Ancestor Photos (Placeholder) -->
    <section class="ancestor-photos page-break-after">
      <h2>Di Ảnh Tiền Nhân</h2>
      <div class="photo-grid">
        <div v-for="i in 4" :key="i" class="photo-item">
          <img :src="`https://via.placeholder.com/150?text=Ancestor+${i}`" alt="Ancestor Photo" class="ancestor-photo" />
          <div class="photo-caption">
            <span class="member-name">Nguyễn Văn A</span><br>
            <span class="member-detail">(1900 - 1970) - Đời thứ {{ i }}</span>
          </div>
        </div>
      </div>
    </section>

    <!-- Family Tree Visualization (Placeholder) -->
    <section class="family-tree-visualization page-break-after">
      <h2>Thế Thứ Đồ Dòng Họ {{ family.name }}</h2>
      <div class="tree-placeholder">
        <p>Biểu đồ cây gia phả sẽ được hiển thị ở đây (sử dụng SVG hoặc Image)</p>
        <img src="https://via.placeholder.com/800x600?text=Family+Tree+Diagram" alt="Family Tree Diagram" class="tree-diagram" />
      </div>
    </section>

    <!-- Family Members List -->
    <section class="family-members page-break-after">
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
    <section class="timeline page-break-after">
      <h2>Dòng Thời Gian Các Sự Kiện</h2>
      <p>Dòng thời gian các sự kiện quan trọng trong gia đình sẽ được hiển thị ở đây.</p>
      <img src="https://via.placeholder.com/600x200?text=Timeline" alt="Timeline Placeholder" class="timeline-image" />
    </section>

    <!-- Stories & Biography (Placeholder) -->
    <section class="stories-biography page-break-after">
      <h2>Câu Chuyện & Tiểu Sử</h2>
      <h3>Cụ Tổ Khảo: {{ family.name }} {{ family.members[0]?.fullName || 'Người thứ nhất' }}</h3>
      <div class="story-content">
        <p class="dropcap-paragraph">
          <span class="dropcap">T</span>rong dòng chảy lịch sử đầy biến động của dân tộc, họ {{ family.name }} đã ghi dấu những trang sử vẻ vang. Từ những ngày đầu khai phá, dựng xây cơ nghiệp trên mảnh đất... (Mô tả địa danh), các thế hệ tiền nhân đã không ngừng lao động, sáng tạo và cống hiến.
        </p>
        <p>
          Câu chuyện về cụ tổ khảo {{ family.name }} {{ family.members[0]?.fullName || 'Người thứ nhất' }} là một minh chứng hùng hồn cho tinh thần kiên cường, ý chí vươn lên và lòng yêu quê hương, đất nước sâu sắc. Cụ không chỉ là người đặt nền móng vững chắc cho dòng họ mà còn là tấm gương sáng về đạo đức, trí tuệ và lòng nhân ái.
        </p>
        <img src="https://via.placeholder.com/200x150?text=Story+Image" alt="Story Illustration" class="story-image" style="float: left; margin-right: 15px;">
        <p>
          Sinh thời, cụ... (Mô tả chi tiết về cuộc đời, sự nghiệp, những đóng góp của cụ). Những lời dạy của cụ, những bài học kinh nghiệm quý báu đã trở thành kim chỉ nam cho các thế hệ con cháu noi theo. Gia phả không chỉ là nơi lưu giữ những thông tin khô khan mà còn là kho tàng những câu chuyện cảm động, những bài học sâu sắc về tình người, tình thân.
        </p>
        <blockquote class="pull-quote">
          "Cội nguồn vững chắc, hậu thế hưng vượng. Ghi nhớ công ơn tổ tiên, gìn giữ gia phong."
        </blockquote>
        <p>
          Ngày nay, con cháu họ {{ family.name }} trên khắp mọi miền đất nước và ở hải ngoại vẫn luôn tự hào về dòng tộc mình, không ngừng phấn đấu học tập, lao động và cống hiến để làm rạng danh dòng họ, xứng đáng với công ơn của tổ tiên.
        </p>
      </div>
    </section>

    <!-- Family Map (Placeholder) -->
    <section class="family-map page-break-after">
      <h2>Dấu Chân Dòng Họ</h2>
      <div class="map-placeholder">
        <p>Bản đồ phân bố và di cư của dòng họ sẽ được hiển thị ở đây.</p>
        <img src="https://via.placeholder.com/800x500?text=Family+Map" alt="Family Map Placeholder" class="map-image" />
      </div>
    </section>

    <!-- Ensure the last page does not have a page-break-after if it's the very last content -->
    <section class="last-section">
      <!-- Content for the last section, ensuring no page break -->
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

const currentYear = computed(() => {
  return new Date().getFullYear();
});

// A simplified way to get lunar year (approximation)
const currentYearLunar = computed(() => {
  const year = new Date().getFullYear();
  const lunarYears = ["Giáp Tý", "Ất Sửu", "Bính Dần", "Đinh Mão", "Mậu Thìn", "Kỷ Tỵ", "Canh Ngọ", "Tân Mùi", "Nhâm Thân", "Quý Dậu", "Giáp Tuất", "Ất Hợi", "Bính Tý", "Đinh Sửu", "Mậu Dần", "Kỷ Mão", "Canh Thìn", "Tân Tỵ", "Nhâm Ngọ", "Quý Mùi", "Giáp Thân", "Ất Dậu", "Bính Tuất", "Đinh Hợi", "Mậu Tý", "Kỷ Sửu", "Canh Dần", "Tân Mão", "Nhâm Thìn", "Quý Tỵ", "Giáp Ngọ", "Ất Mùi", "Bính Thân", "Đinh Dậu", "Mậu Tuất", "Kỷ Hợi", "Canh Tý", "Tân Sửu", "Nhâm Dần", "Quý Mão", "Giáp Thìn"];
  return lunarYears[(year - 1984) % 60]; // Cycle through the 60-year cycle
});
</script>

<style>
/* Add Google Fonts import - ensure these are available in Puppeteer's environment or bundled */
@import url('https://fonts.googleapis.com/css2?family=Playfair+Display:wght@400;700&family=Open+Sans:wght@400;700&display=swap');

/* Base styles for the entire PDF template */
.family-tree-pdf-template {
  font-family: 'Open Sans', 'Roboto', 'Montserrat', sans-serif; /* Sans-serif for body */
  color: #3E3B36; /* Dark Sepia */
  margin: 15mm 20mm; /* Standard print margin, slightly less top/bottom */
  line-height: 1.6;
  font-size: 11pt; /* Slightly smaller body font */
  background-color: #F8F5E6; /* Off-white / Aged Cream */
  box-sizing: border-box; /* Include padding and border in the element's total width and height */
}

/* Page break handling */
.page-break-after {
  page-break-after: always;
}

.family-tree-pdf-template section:last-of-type {
  page-break-after: auto; /* Ensure the very last section does not have an extra page break */
}

h1, h2, h3 {
  font-family: 'Playfair Display', 'Noto Serif VN', 'Merriweather', serif; /* Serif for headings */
  color: #B39B5E; /* Muted Gold for general headings */
  margin-top: 1.5em;
  margin-bottom: 0.8em;
  text-align: center; /* Center all headings by default */
}

h1.main-title {
  font-size: 48pt; /* Extra large */
  font-weight: 700; /* Bold */
  color: #800020; /* Deep Burgundy for main title */
  margin-bottom: 30mm;
  text-transform: uppercase;
  letter-spacing: 2px;
  line-height: 1.1;
}

h2 {
  font-size: 20pt;
  font-weight: 700;
  padding-bottom: 8px;
  position: relative;
  margin-bottom: 1.5em;
}

h2::after {
  content: '';
  display: block;
  width: 50%; /* Center the line */
  margin: 8px auto 0 auto;
  border-bottom: 2px solid #B39B5E; /* Muted Gold line */
}

h3 {
  font-size: 16pt;
  font-weight: 700;
  color: #3E3B36; /* Dark Sepia for sub-headings */
  text-align: left; /* Align sub-headings left */
  margin-left: 20mm; /* Indent sub-headings */
}

/* Decorative Divider */
.decorative-divider {
  border-top: 1px solid #B39B5E;
  margin: 2em auto;
  width: 70%;
  position: relative;
}
.decorative-divider::before, .decorative-divider::after {
  content: '';
  position: absolute;
  top: -3px;
  width: 6px;
  height: 6px;
  background-color: #B39B5E;
  transform: rotate(45deg);
}
.decorative-divider::before { left: 0; }
.decorative-divider::after { right: 0; }


/* Cover Page Styles */
.cover-page {
  background-color: #4A4238; /* Darker Sepia background */
  color: #F8F5E6; /* Off-white text */
  height: 297mm; /* A4 height */
  display: flex;
  flex-direction: column;
  justify-content: center;
  align-items: center;
  padding: 0;
  margin: 0;
  text-align: center;
}

.cover-page .header-text {
  font-family: 'Playfair Display', serif;
  font-size: 14pt;
  color: #B39B5E; /* Muted Gold */
  margin-bottom: 10mm;
  letter-spacing: 1px;
}

.cover-page .subtitle {
  font-family: 'Playfair Display', serif;
  font-size: 18pt;
  font-style: italic;
  color: #F8F5E6;
  margin-top: 15mm;
  margin-bottom: 20mm;
}

.cover-page .footer-text {
  font-family: 'Open Sans', sans-serif;
  font-size: 10pt;
  color: #F8F5E6;
  position: absolute;
  bottom: 20mm;
  width: 100%;
  text-align: center;
}

.central-emblem {
  position: relative;
  width: 100%;
  text-align: center;
  margin-top: 20mm;
  margin-bottom: 20mm;
}

.emblem-image {
  max-width: 200px;
  height: auto;
  border: 5px double #B39B5E; /* Muted Gold double border */
  border-radius: 50%; /* Circular */
  box-shadow: 0 0 15px rgba(0,0,0,0.3);
}

.nomen-han-script {
    font-family: serif; /* Fallback for Han Nom */
    font-size: 72pt;
    color: #B39B5E;
    position: absolute;
    top: 50%;
    left: 50%;
    transform: translate(-50%, -50%);
    opacity: 0.8;
    /* This needs a specific Han Nom font to render correctly */
}

/* Foreword Page Styles */
.foreword-page {
  text-align: justify;
  padding: 15mm 20mm;
  line-height: 1.8;
}

.foreword-page h2 {
  color: #800020; /* Deep Burgundy */
  text-align: center;
  margin-bottom: 2em;
}

.foreword-content {
  font-family: 'Open Sans', sans-serif;
  font-size: 12pt;
  text-indent: 1.5em;
  margin-bottom: 2em;
}

.author-info {
  text-align: right;
  font-style: italic;
  margin-top: 3em;
  font-size: 10pt;
}


/* Ancestor Photos Styles */
.ancestor-photos h2 {
    color: #B39B5E;
}
.ancestor-photos .photo-grid {
  display: grid;
  grid-template-columns: repeat(2, 1fr); /* 2 columns */
  gap: 20mm; /* Larger gap */
  justify-items: center;
  margin-top: 20mm;
}

.photo-item {
    text-align: center;
    break-inside: avoid; /* Keep photo and caption together */
}

.ancestor-photo {
  max-width: 120mm; /* Max width for a 2-column layout */
  height: auto;
  border: 3px double #B39B5E; /* Muted Gold double border */
  box-shadow: 3px 3px 8px rgba(0,0,0,0.3);
  filter: sepia(0.3) brightness(0.9); /* Subtle sepia tone */
  margin-bottom: 10px;
}

.photo-caption {
  font-family: 'Open Sans', sans-serif;
  font-size: 9pt;
  color: #3E3B36;
}
.photo-caption .member-name {
  font-weight: bold;
  font-size: 10pt;
}
.photo-caption .member-detail {
  font-style: italic;
}

/* Family Tree Visualization Styles */
.family-tree-visualization h2 {
    color: #B39B5E;
}
.family-tree-visualization .tree-placeholder {
  text-align: center;
  margin-top: 20mm;
}
.family-tree-visualization .tree-diagram {
  max-width: 100%;
  height: auto;
  border: 1px solid #ccc; /* Simple border for placeholder */
  /* This will ideally be an SVG or image generated externally */
}

/* Family Members Table Styles */
.family-members h2 {
    color: #B39B5E;
}
.family-members table {
  width: 100%;
  border-collapse: collapse;
  margin-top: 15mm;
  font-family: 'Open Sans', sans-serif;
  font-size: 10pt;
  color: #3E3B36;
}

.family-members th, .family-members td {
  border: 1px solid #B39B5E; /* Muted Gold border */
  padding: 8px 12px;
  text-align: left;
}

.family-members th {
  background-color: #EFEBE0; /* Lighter cream background */
  font-weight: bold;
  color: #4A4238;
}

/* Timeline Styles */
.timeline h2 {
    color: #B39B5E;
}
.timeline p {
  margin-top: 15mm;
  font-style: italic;
  color: #4A4238;
}
.timeline-image {
  max-width: 100%;
  height: auto;
  border: 1px solid #ddd;
}

/* Stories & Biography Styles */
.stories-biography h2 {
    color: #B39B5E;
}
.stories-biography h3 {
    text-align: center;
    color: #800020; /* Deep Burgundy */
    margin-bottom: 1.5em;
}
.story-content {
    font-family: 'Open Sans', sans-serif;
    font-size: 11pt;
    line-height: 1.7;
    text-align: justify;
    padding: 0 10mm;
}
.dropcap-paragraph {
    position: relative;
    text-indent: 0;
    margin-top: 1em;
}
.dropcap {
    float: left;
    font-size: 4em; /* Large initial letter */
    line-height: 0.8;
    font-family: 'Playfair Display', serif;
    color: #B39B5E; /* Muted Gold */
    margin-right: 8px;
    margin-top: 0.05em;
    font-weight: bold;
}
.story-image {
    max-width: 40%;
    height: auto;
    border: 3px double #B39B5E;
    box-shadow: 2px 2px 5px rgba(0,0,0,0.2);
    filter: sepia(0.2);
    margin: 10px;
}
.pull-quote {
    font-family: 'Playfair Display', serif;
    font-size: 14pt;
    font-style: italic;
    color: #4A4238;
    text-align: center;
    margin: 2em auto;
    padding: 1em 2em;
    border-left: 5px solid #B39B5E;
    border-right: 5px solid #B39B5E;
    max-width: 80%;
}

/* Family Map Styles */
.family-map h2 {
    color: #B39B5E;
}
.map-placeholder {
  text-align: center;
  margin-top: 20mm;
  background-color: #EFEBE0; /* Light cream background for map */
  padding: 10mm;
  border: 1px solid #B39B5E;
}
.map-image {
  max-width: 100%;
  height: auto;
}

/* Generic page break for sections, ensuring consistent spacing */
.family-tree-pdf-template section {
  margin-bottom: 15mm; /* Space between sections */
}

</style>