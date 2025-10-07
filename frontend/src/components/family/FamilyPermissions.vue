<template>
  <div>
    <v-autocomplete
      v-model="managers"
      :items="userProfiles"
      item-title="name"
      item-value="id"
      chips
      closable-chips
      multiple
      :label="t('family.permissions.managers')"
    ></v-autocomplete>

    <v-autocomplete
      v-model="viewers"
      :items="userProfiles"
      item-title="name"
      item-value="id"
      chips
      closable-chips
      multiple
      :label="t('family.permissions.viewers')"
    ></v-autocomplete>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, computed } from 'vue';
import { useI18n } from 'vue-i18n';
import { useUserProfileStore } from '@/stores/userProfile.store';
import { storeToRefs } from 'pinia';
import type { FamilyUser } from '@/types/family';

const { t } = useI18n();

const props = defineProps<{
  modelValue: FamilyUser[];
}>();

const emit = defineEmits(['update:modelValue']);

const userProfileStore = useUserProfileStore();
const { userProfiles } = storeToRefs(userProfileStore);

onMounted(() => {
  userProfileStore.fetchUserProfiles();
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
