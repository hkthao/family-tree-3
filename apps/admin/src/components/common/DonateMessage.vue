<template>
  <v-alert
    v-model="showDonateMessage"
    type="info"
    closable
    class="ma-4"
    variant="tonal"
  >
    Ủng hộ tác giả ☕️
    <br>
    Mình xây dựng app Gia Phả Huỳnh như một dự án cộng đồng, không quảng cáo, không tính phí.
    <br>
    Nếu bạn thấy app hữu ích, hãy ủng hộ mình một khoản nhỏ để duy trì server và phát triển thêm tính năng mới.
    <br>
    Cảm ơn bạn nhiều ❤️
    <template v-slot:append>
      <v-btn
        variant="text"
        color="primary"
        :to="{ name: 'Donate' }"
      >
        Ủng hộ ngay
      </v-btn>
    </template>
  </v-alert>
</template>

<script setup lang="ts">
import { ref, onMounted, watch } from 'vue';

const showDonateMessage = ref(true);
const DONATE_MESSAGE_DISMISSED_KEY = 'donateMessageDismissed';

onMounted(() => {
  // Check local storage for dismissal preference
  const dismissed = localStorage.getItem(DONATE_MESSAGE_DISMISSED_KEY);
  if (dismissed === 'true') {
    showDonateMessage.value = false;
  }
});

// Watch for changes in showDonateMessage and save to local storage
watch(showDonateMessage, (newValue) => {
  localStorage.setItem(DONATE_MESSAGE_DISMISSED_KEY, String(newValue === false));
});
</script>

<style scoped>
/* Add any specific styles for the donate message here */
</style>
