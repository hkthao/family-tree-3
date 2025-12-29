<template>
  <v-card flat>
    <v-card-title class="d-flex align-center pe-2">
      <v-icon icon="mdi-shield-lock"></v-icon> &nbsp;{{ t('family.privacy.title') }}
    </v-card-title>
    <v-card-text>
      <p class="mb-4">{{ t('family.privacy.description') }}</p>

      <v-alert type="info" variant="tonal" class="mb-4">
        {{ t('family.privacy.infoMessage') }}
      </v-alert>

      <v-form ref="formRef">
        <v-expansion-panels variant="accordion" class="mb-4">
          <v-expansion-panel v-for="(group, groupName) in privacyGroups" :key="groupName">
            <v-expansion-panel-title class="font-weight-medium">
              {{ group.title }}
              <v-spacer></v-spacer>
              <v-btn v-if="!allChecked(groupName)" density="compact" flat @click.stop="checkAll(groupName)">
                {{ t('common.checkAll') }}
              </v-btn>
              <v-btn v-else density="compact" flat @click.stop="uncheckAll(groupName)">
                {{ t('common.uncheckAll') }}
              </v-btn>
            </v-expansion-panel-title>
            <v-expansion-panel-text>
              <v-row>
                <v-col cols="12" sm="6" md="3" v-for="prop in group.properties" :key="prop.value">
                  <v-checkbox v-model="selectedProperties[groupName]" :label="prop.text" :value="prop.value"
                    hide-details density="compact"
                    :indeterminate="selectedProperties[groupName]?.length > 0 && selectedProperties[groupName]?.length < group.properties.length"></v-checkbox>
                </v-col>
              </v-row>
            </v-expansion-panel-text>
          </v-expansion-panel>
        </v-expansion-panels>
      </v-form>
    </v-card-text>
    <v-card-actions>
      <v-spacer></v-spacer>
      <v-btn color="primary" :loading="isLoading || isUpdating" @click="savePrivacySettings"
        data-testid="save-privacy-settings-button">
        {{ t('common.save') }}
      </v-btn>
    </v-card-actions>
  </v-card>
</template>

<script setup lang="ts">
import { ref, watch, computed, toRef, reactive } from 'vue';
import { useI18n } from 'vue-i18n';
import { useGlobalSnackbar } from '@/composables';
import { usePrivacyConfiguration } from '@/composables/family/usePrivacyConfiguration';
import {
  MEMBER_PROPS,
  FAMILY_PROPS,
  EVENT_PROPS,
  FAMILY_LOCATION_PROPS,
  MEMORY_ITEM_PROPS,
  MEMBER_FACE_PROPS,
  FOUND_FACE_PROPS,
} from '@/constants/privacy-props.constants';

const props = defineProps<{
  familyId: string;
}>();

const { t } = useI18n();
const { showSnackbar } = useGlobalSnackbar();

const {
  state: { privacyConfiguration, isLoading, isUpdating },
  actions: { updatePrivacySettings },
} = usePrivacyConfiguration(toRef(props, 'familyId'));

// Use a reactive object to store selected properties for each group
interface SelectedProperties {
  [key: string]: string[];
}
const selectedProperties = reactive<SelectedProperties>({});
const formRef = ref<HTMLFormElement | null>(null);

// Define grouped properties based on constants
const privacyGroups = computed(() => ({
  member: {
    title: t('family.privacy.groups.member'),
    properties: MEMBER_PROPS.PROPERTIES.map((p: string) => ({
      text: t(`member.form.${p}`), // Assuming translation keys like member.form.firstName
      value: p,
    })),
  },
  family: {
    title: t('family.privacy.groups.family'),
    properties: FAMILY_PROPS.PROPERTIES.map((p: string) => ({
      text: t(`family.form.${p}`), // Assuming translation keys like family.form.name
      value: p,
    })),
  },
  event: {
    title: t('family.privacy.groups.event'),
    properties: EVENT_PROPS.PROPERTIES.map((p: string) => ({
      text: t(`event.form.${p}`), // Assuming translation keys like event.form.name
      value: p,
    })),
  },
  familyLocation: {
    title: t('family.privacy.groups.familyLocation'),
    properties: FAMILY_LOCATION_PROPS.PROPERTIES.map((p: string) => ({
      text: t(`familyLocation.form.${p}`), // Assuming translation keys like familyLocation.form.name
      value: p,
    })),
  },
  memoryItem: {
    title: t('family.privacy.groups.memoryItem'),
    properties: MEMORY_ITEM_PROPS.PROPERTIES.map((p: string) => ({
      text: t(`memoryItem.form.${p}`), // Assuming translation keys like memoryItem.form.title
      value: p,
    })),
  },
  memberFace: {
    title: t('family.privacy.groups.memberFace'),
    properties: MEMBER_FACE_PROPS.PROPERTIES.map((p: string) => ({
      text: t(`memberFace.form.${p}`), // Assuming translation keys
      value: p,
    })),
  },
  foundFace: {
    title: t('family.privacy.groups.foundFace'),
    properties: FOUND_FACE_PROPS.PROPERTIES.map((p: string) => ({
      text: t(`foundFace.form.${p}`), // Assuming translation keys
      value: p,
    })),
  },
}));

// Watch for changes in privacyConfiguration fetched by the composable
watch(privacyConfiguration, (newConfig) => {
  if (newConfig) {
    // Populate selectedProperties from the new config, mapping to entity-specific arrays
    selectedProperties.member = newConfig.publicMemberProperties || [];
    selectedProperties.family = newConfig.publicFamilyProperties || [];
    selectedProperties.event = newConfig.publicEventProperties || [];
    selectedProperties.familyLocation = newConfig.publicFamilyLocationProperties || [];
    selectedProperties.memoryItem = newConfig.publicMemoryItemProperties || [];
    selectedProperties.memberFace = newConfig.publicMemberFaceProperties || [];
    selectedProperties.foundFace = newConfig.publicFoundFaceProperties || [];
  } else {
    // If no config exists, default to all properties being public for each group
    (Object.keys(privacyGroups.value) as (keyof typeof privacyGroups.value)[]).forEach(groupName => {
      selectedProperties[groupName] = privacyGroups.value[groupName].properties.map((p: { text: string; value: string }) => p.value);
    });
  }
}, { immediate: true });

const checkAll = (groupName: keyof typeof privacyGroups.value) => {
  selectedProperties[groupName] = privacyGroups.value[groupName].properties.map((p: { text: string; value: string }) => p.value);
};

const uncheckAll = (groupName: keyof typeof privacyGroups.value) => {
  selectedProperties[groupName] = [];
};

const allChecked = (groupName: keyof typeof privacyGroups.value) => {
  return selectedProperties[groupName]?.length === privacyGroups.value[groupName].properties.length;
};

const savePrivacySettings = async () => {
  // Flatten the selectedProperties object into a single object with entity-specific arrays
  const settingsToSave = {
    familyId: props.familyId,
    publicMemberProperties: selectedProperties.member,
    publicFamilyProperties: selectedProperties.family,
    publicEventProperties: selectedProperties.event,
    publicFamilyLocationProperties: selectedProperties.familyLocation,
    publicMemoryItemProperties: selectedProperties.memoryItem,
    publicMemberFaceProperties: selectedProperties.memberFace,
    publicFoundFaceProperties: selectedProperties.foundFace,
  };
  try {
    await updatePrivacySettings(settingsToSave);
    showSnackbar(t('family.privacy.saveSuccess'), 'success');
  } catch (err: any) {
    showSnackbar(t('family.privacy.saveError'), 'error');
    console.error('Error saving privacy settings:', err);
  }
};
</script>

<style scoped>
/* Add any specific styles here */
</style>
