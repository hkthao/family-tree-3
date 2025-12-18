<template>
 <div></div>
</template>

<script setup lang="ts">
import { computed, ref, watch } from 'vue';
import { useI18n } from 'vue-i18n';

const props = defineProps<{
  search: string;
}>();

const { t } = useI18n();

</script>
