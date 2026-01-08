<template>
  <v-card :elevation="0">
    <v-card-text class="pa-0">
      <v-container fluid class="pa-4" :class="{'loading-overlay': isLoading}">
        <v-overlay v-model="isLoading" contained class="align-center justify-center">
          <v-progress-circular indeterminate color="primary"></v-progress-circular>
        </v-overlay>
        <v-row v-if="!isFollowing" no-gutters class="mb-4">
          <v-col cols="12">
            <v-alert
              type="info"
              variant="tonal"
              :text="t('family.followSettings.infoMessage')"
            ></v-alert>
          </v-col>
        </v-row>

        <FamilyFollowForm
          ref="familyFollowFormRef"
          :family-id="props.familyId"
          :is-following="isFollowing"
          :initial-notify-death-anniversary="notifyDeathAnniversary"
          :initial-notify-birthday="notifyBirthday"
          :initial-notify-event="notifyEvent"
          :is-loading="isLoading"
          @saved="handleFormSaved"
          @close="emit('close')"
        />
      </v-container>
    </v-card-text>
    <v-card-actions>
      <v-spacer></v-spacer>
      <v-btn color="grey" data-testid="button-cancel" @click="emit('close')" :disabled="isLoading">
        {{ t('common.cancel') }}
      </v-btn>
      <v-btn
        v-if="isFollowing"
        color="warning"
        data-testid="button-unfollow"
        @click="unfollowFamily"
        :loading="isLoading"
        :disabled="isLoading"
        class="ml-2"
      >
        {{ t('family.unfollow') }}
      </v-btn>
      <v-btn color="primary" data-testid="button-save" @click="saveForm" :loading="isLoading" :disabled="isLoading" class="ml-2">
        {{ t('common.save') }}
      </v-btn>
    </v-card-actions>
  </v-card>
</template>

<script setup lang="ts">
import { ref, watch, onMounted, computed } from 'vue';
import { useI18n } from 'vue-i18n';
import { useGlobalSnackbar } from '@/composables/ui/useGlobalSnackbar';
import { useFamilyFollow } from '@/composables/family/logic/useFamilyFollow';
import FamilyFollowForm from '@/components/family/FamilyFollowForm.vue';
import type { Result } from '@/types';

// Define the interface for the FamilyFollowForm component's exposed methods
interface FamilyFollowFormInstance {
  saveSettings: () => Promise<Result<string | boolean>>;
  notifyDeathAnniversary: boolean;
  notifyBirthday: boolean;
  notifyEvent: boolean;
}

const props = defineProps<{
  familyId: string;
  isFollowingInitial: boolean; // Initial state passed from parent
}>();

const emit = defineEmits(['saved', 'close', 'update:title']);

const { t } = useI18n();
const { showSnackbar } = useGlobalSnackbar();

const isLoading = ref(false);
const isFollowing = ref(props.isFollowingInitial);

const familyFollowFormRef = ref<FamilyFollowFormInstance | null>(null);

// Reactive variables to hold the current settings, fetched from backend
const notifyDeathAnniversary = ref(true);
const notifyBirthday = ref(true);
const notifyEvent = ref(true);

const { state: familyFollowState, actions: familyFollowActions } = useFamilyFollow(ref(props.familyId));

// Computed property for the drawer title
const drawerTitle = computed(() => {
  return isFollowing.value ? t('family.followSettings.title') : t('family.followSettings.newFollow');
});

onMounted(async () => {
  isLoading.value = true;
  await familyFollowActions.fetchFollowStatus();
  isLoading.value = false;

  if (familyFollowState.error.value) {
    showSnackbar(familyFollowState.error.value.message || t('family.followSettings.errorLoadingSettings'), 'error');
    isFollowing.value = false;
  } else if (familyFollowState.familyFollowData.value) {
    isFollowing.value = familyFollowState.familyFollowData.value.isFollowing;
    notifyDeathAnniversary.value = familyFollowState.familyFollowData.value.notifyDeathAnniversary;
    notifyBirthday.value = familyFollowState.familyFollowData.value.notifyBirthday;
    notifyEvent.value = familyFollowState.familyFollowData.value.notifyEvent;
  } else {
    isFollowing.value = false;
  }
  emit('update:title', drawerTitle.value); // Emit title on mount
});

const handleFormSaved = () => {
  emit('saved');
  familyFollowActions.fetchFollowStatus();
};

const saveForm = async () => {
  if (familyFollowFormRef.value) {
    isLoading.value = true;
    const result = await familyFollowFormRef.value.saveSettings();
    isLoading.value = false;

    if (result && result.ok) {
      showSnackbar(t('family.followSettings.saveSuccess'), 'success');
      emit('saved');
      if (familyFollowFormRef.value) {
        notifyDeathAnniversary.value = familyFollowFormRef.value.notifyDeathAnniversary;
        notifyBirthday.value = familyFollowFormRef.value.notifyBirthday;
        notifyEvent.value = familyFollowFormRef.value.notifyEvent;
      }
      await familyFollowActions.fetchFollowStatus();
    } else {
      showSnackbar(result?.error?.message || t('family.followSettings.saveError'), 'error');
    }
  }
};

const unfollowFamily = async () => {
  isLoading.value = true;
  const result = await familyFollowActions.unfollowFamily();
  isLoading.value = false;

  if (result && result.ok) {
    showSnackbar(t('family.unfollowSuccess'), 'success');
    emit('saved');
  } else {
    showSnackbar(result?.error?.message || t('family.unfollowError'), 'error');
  }
};

watch(isFollowing, () => {
  emit('update:title', drawerTitle.value); // Emit title when isFollowing changes
});

watch(() => familyFollowState.isFollowing.value, (newValue) => {
  isFollowing.value = newValue;
});

watch(() => props.familyId, async (newFamilyId) => {
  if (newFamilyId) {
    isLoading.value = true;
    await familyFollowActions.fetchFollowStatus();
    isLoading.value = false;
    if (familyFollowState.error.value) {
      showSnackbar(familyFollowState.error.value.message || t('family.followSettings.errorLoadingSettings'), 'error');
      isFollowing.value = false;
    } else if (familyFollowState.familyFollowData.value) {
      isFollowing.value = familyFollowState.familyFollowData.value.isFollowing;
      notifyDeathAnniversary.value = familyFollowState.familyFollowData.value.notifyDeathAnniversary;
      notifyBirthday.value = familyFollowState.familyFollowData.value.notifyBirthday;
      notifyEvent.value = familyFollowState.familyFollowData.value.notifyEvent;
    } else {
      isFollowing.value = false;
    }
  }
});
</script>
<style scoped></style>