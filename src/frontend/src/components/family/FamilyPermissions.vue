<template>
  <v-row>
    <v-col>
      <UserAutocomplete v-model="managers" chips closable-chips multiple :disabled="props.readOnly"
        :label="t('family.permissions.managers')" data-testid="family-managers-select"></UserAutocomplete>
    </v-col>
    <v-col>
      <UserAutocomplete v-model="viewers" chips closable-chips multiple :disabled="props.readOnly"
        :label="t('family.permissions.viewers')" data-testid="family-viewers-select"></UserAutocomplete>
    </v-col>
  </v-row>
</template>

<script setup lang="ts">
import { computed } from 'vue';
import { useI18n } from 'vue-i18n';
import type { FamilyUser } from '@/types';
import UserAutocomplete from '@/components/common/UserAutocomplete.vue';

const { t } = useI18n();

const props = defineProps<{
  modelValue: FamilyUser[];
  readOnly?: boolean;
}>();

const emit = defineEmits(['update:modelValue']);

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
