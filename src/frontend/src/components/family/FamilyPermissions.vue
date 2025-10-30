<template>
  <v-row>
    <v-col>
      <v-autocomplete v-model="managers" :items="userProfiles" item-title="name" item-value="id" chips closable-chips
        multiple :disabled="props.readOnly" :label="t('family.permissions.managers')"
        data-testid="family-managers-select"></v-autocomplete> </v-col>
    <v-col>
      <v-autocomplete v-model="viewers" :items="userProfiles" item-title="name" item-value="id" chips closable-chips
        multiple :disabled="props.readOnly" :label="t('family.permissions.viewers')"
        data-testid="family-viewers-select"></v-autocomplete> </v-col>
  </v-row>
</template>

<script setup lang="ts">
import { onMounted, computed } from 'vue';
import { useI18n } from 'vue-i18n';
import { useUserProfileStore } from '@/stores';
import { storeToRefs } from 'pinia';
import type { FamilyUser } from '@/types';

const { t } = useI18n();

const props = defineProps<{
  modelValue: FamilyUser[];
  readOnly?: boolean;
}>();

const emit = defineEmits(['update:modelValue']);

const userProfileStore = useUserProfileStore();
const { allUserProfiles: userProfiles } = storeToRefs(userProfileStore);

onMounted(() => {
  userProfileStore.fetchAllUserProfiles();
});

const managers = computed({
  get: () => props.modelValue.filter(fu => fu.role === 'Manager').map(fu => fu.userProfileId),
  set: (newUserProfileIds) => {
    const newManagers = newUserProfileIds.map(id => ({ familyId: '', userProfileId: id, role: 'Manager' }));
    const otherUsers = props.modelValue.filter(fu => fu.role !== 'Manager');
    emit('update:modelValue', [...otherUsers, ...newManagers]);
  }
});

const viewers = computed({
  get: () => props.modelValue.filter(fu => fu.role === 'Viewer').map(fu => fu.userProfileId),
  set: (newUserProfileIds) => {
    const newViewers = newUserProfileIds.map(id => ({ familyId: '', userProfileId: id, role: 'Viewer' }));
    const otherUsers = props.modelValue.filter(fu => fu.role !== 'Viewer');
    emit('update:modelValue', [...otherUsers, ...newViewers]);
  }
});
</script>
