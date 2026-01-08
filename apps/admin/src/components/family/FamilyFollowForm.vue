<template>
  <v-list density="compact">
    <v-list-item>
      <template v-slot:prepend>
        <v-icon icon="mdi-grave-stone"></v-icon>
      </template>
      <v-list-item-title>{{ t('family.followSettings.notifyDeathAnniversary') }}</v-list-item-title>
      <v-list-item-subtitle>{{ t('family.followSettings.notifyDeathAnniversaryDescription') }}</v-list-item-subtitle>
      <template v-slot:append>
        <v-switch
          v-model="notifyDeathAnniversary"
          color="primary"
          inset
          data-testid="switch-notify-death-anniversary"
          hide-details
        ></v-switch>
      </template>
    </v-list-item>

    <v-divider></v-divider>

    <v-list-item>
      <template v-slot:prepend>
        <v-icon icon="mdi-cake"></v-icon>
      </template>
      <v-list-item-title>{{ t('family.followSettings.notifyBirthday') }}</v-list-item-title>
      <v-list-item-subtitle>{{ t('family.followSettings.notifyBirthdayDescription') }}</v-list-item-subtitle>
      <template v-slot:append>
        <v-switch
          v-model="notifyBirthday"
          color="primary"
          inset
          data-testid="switch-notify-birthday"
          hide-details
        ></v-switch>
      </template>
    </v-list-item>

    <v-divider></v-divider>

    <v-list-item>
      <template v-slot:prepend>
        <v-icon icon="mdi-calendar-alert"></v-icon>
      </template>
      <v-list-item-title>{{ t('family.followSettings.notifyEvent') }}</v-list-item-title>
      <v-list-item-subtitle>{{ t('family.followSettings.notifyEventDescription') }}</v-list-item-subtitle>
      <template v-slot:append>
        <v-switch
          v-model="notifyEvent"
          color="primary"
          inset
          data-testid="switch-notify-event"
          hide-details
        ></v-switch>
      </template>
    </v-list-item>
  </v-list>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import { useFamilyFollow } from '@/composables/family/logic/useFamilyFollow';
import type { UpdateFamilyFollowSettingsCommand, FollowFamilyCommand } from '@/types/familyFollow';
import type { Result } from '@/types';

const props = defineProps<{
  familyId: string;
  isFollowing: boolean; // Current following status
  initialNotifyDeathAnniversary: boolean;
  initialNotifyBirthday: boolean;
  initialNotifyEvent: boolean;
  isLoading: boolean; // Loading state from parent (for display purposes within form if needed)
}>();

const { t } = useI18n();

const notifyDeathAnniversary = ref(props.initialNotifyDeathAnniversary);
const notifyBirthday = ref(props.initialNotifyBirthday);
const notifyEvent = ref(props.initialNotifyEvent);

// Watch for changes in initial props to update local refs if parent changes them
watch(() => props.initialNotifyDeathAnniversary, (newValue) => { notifyDeathAnniversary.value = newValue; });
watch(() => props.initialNotifyBirthday, (newValue) => { notifyBirthday.value = newValue; });
watch(() => props.initialNotifyEvent, (newValue) => { notifyEvent.value = newValue; });


const { actions: familyFollowActions } = useFamilyFollow(ref(props.familyId));

const saveSettings = async (): Promise<Result<string | boolean>> => {
  let result: Result<string | boolean>;

  if (props.isFollowing) {
    // Update existing follow settings
    const command: UpdateFamilyFollowSettingsCommand = {
      familyId: props.familyId,
      notifyDeathAnniversary: notifyDeathAnniversary.value,
      notifyBirthday: notifyBirthday.value,
      notifyEvent: notifyEvent.value,
    };
    result = await familyFollowActions.updateFamilyFollowSettings(props.familyId, command);
  } else {
    // Start following with new settings
    const command: FollowFamilyCommand = {
      familyId: props.familyId,
      notifyDeathAnniversary: notifyDeathAnniversary.value,
      notifyBirthday: notifyBirthday.value,
      notifyEvent: notifyEvent.value,
    };
    result = await familyFollowActions.followFamily(command);
  }

  return result;
};

defineExpose({
  saveSettings,
  notifyDeathAnniversary: notifyDeathAnniversary.value,
  notifyBirthday: notifyBirthday.value,
  notifyEvent: notifyEvent.value,
});
</script>

<style scoped></style>
