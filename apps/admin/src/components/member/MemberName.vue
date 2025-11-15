<template>
  <span v-if="memberName">{{ memberName }}</span>
  <span v-else-if="memberId" class="text-caption text-medium-emphasis">Loading...</span>
  <span v-else class="text-caption text-medium-emphasis">{{ t('common.na') }}</span>
</template>

<script setup lang="ts">
import { ref, watch, onMounted } from 'vue';
import { useI18n } from 'vue-i18n';
import { useMemberStore } from '@/stores/member.store';

const props = defineProps<{
  memberId?: string;
}>();

const { t } = useI18n();
const memberStore = useMemberStore();
const memberName = ref<string | null>(null);

const fetchMemberName = async (id?: string) => {
  if (id) {
    const member = await memberStore.getMemberById(id);
    memberName.value = member?.fullName || null;
  } else {
    memberName.value = null;
  }
};

watch(() => props.memberId, (newId) => {
  fetchMemberName(newId);
}, { immediate: true });

onMounted(() => {
  fetchMemberName(props.memberId);
});
</script>