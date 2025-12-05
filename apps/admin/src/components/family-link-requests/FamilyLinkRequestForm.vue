<template>
  <v-form ref="formRef">
    <v-row>
      <v-col cols="12" md="6">
        <FamilyAutocomplete
          v-model="formData.requestingFamilyId"
          :label="t('familyLinkRequest.form.requestingFamily')"
          :readonly="true"
          data-testid="requesting-family-field"
        />
      </v-col>
      <v-col cols="12" md="6">
        <FamilyAutocomplete
          v-model="formData.targetFamilyId"
          :label="t('familyLinkRequest.form.targetFamily')"
          :readonly="true"
          data-testid="target-family-field"
        />
      </v-col>
      <v-col cols="12" md="6">
        <v-select
          v-model="formData.status"
          :items="linkStatusOptions"
          :label="t('familyLinkRequest.form.status')"
          :readonly="readOnly || isStatusNotPending(formData.status)"
          data-testid="status-field"
        ></v-select>
      </v-col>
      <v-col cols="12" md="6">
        <v-text-field
          v-model="formData.requestDate"
          :label="t('familyLinkRequest.form.requestDate')"
          :readonly="true"
          data-testid="request-date-field"
        ></v-text-field>
      </v-col>
      <v-col cols="12" md="6">
        <v-text-field
          v-model="formData.responseDate"
          :label="t('familyLinkRequest.form.responseDate')"
          :readonly="true"
          data-testid="response-date-field"
        ></v-text-field>
      </v-col>
    </v-row>
  </v-form>
</template>

<script setup lang="ts">
import { ref, reactive, watch, computed, onMounted } from 'vue';
import { useI18n } from 'vue-i18n';
import type { FamilyLinkRequestDto } from '@/types';
import { LinkStatus } from '@/types';
import FamilyAutocomplete from '@/components/common/FamilyAutocomplete.vue';

interface FamilyLinkRequestFormProps {
  initialFamilyLinkRequestData?: FamilyLinkRequestDto;
  readOnly?: boolean;
}

const props = defineProps<FamilyLinkRequestFormProps>();
const emit = defineEmits(['update:modelValue']); // Not using modelValue directly for complex object

const { t } = useI18n();
const formRef = ref<HTMLFormElement | null>(null);

const formData = reactive<Partial<FamilyLinkRequestDto>>({
  id: '',
  requestingFamilyId: '',
  targetFamilyId: '',
  status: LinkStatus.Pending,
  requestDate: '',
  responseDate: '',
});

const linkStatusOptions = computed(() => [
  { title: t('familyLinkRequest.status.pending'), value: LinkStatus.Pending },
  { title: t('familyLinkRequest.status.approved'), value: LinkStatus.Approved },
  { title: t('familyLinkRequest.status.rejected'), value: LinkStatus.Rejected },
]);

const isStatusNotPending = (status: LinkStatus | undefined) => {
  return props.readOnly || (status !== LinkStatus.Pending);
};

onMounted(() => {
  if (props.initialFamilyLinkRequestData) {
    Object.assign(formData, props.initialFamilyLinkRequestData);
  }
});

watch(
  () => props.initialFamilyLinkRequestData,
  (newData) => {
    if (newData) {
      Object.assign(formData, newData);
    }
  },
  { deep: true }
);

const getFormData = () => {
  return { ...formData };
};

const validate = async () => {
  const { valid } = await formRef.value!.validate();
  return valid;
};

defineExpose({
  getFormData,
  validate,
});
</script>