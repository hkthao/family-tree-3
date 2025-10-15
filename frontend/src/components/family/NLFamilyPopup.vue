<template>
  <v-dialog :model-value="modelValue" @update:model-value="$emit('update:modelValue', $event)" max-width="800px">
    <v-card>
      <v-card-title class="headline">{{ t('aiInput.title') }}</v-card-title>
      <v-card-text>
        <v-form ref="form">
          <v-textarea v-model="prompt" :label="t('aiInput.promptLabel')" rows="3" outlined clearable counter
            :auto-grow="true" :rules="[rules.required, rules.length(1000)]"></v-textarea>
        </v-form>
        <v-btn color="primary" :loading="loading" :disabled="loading" @click="generateData" class="mb-4">
          {{ t('aiInput.generateButton') }}
        </v-btn>

        <div v-if="generatedData && generatedData.length">
          <v-alert type="info" class="mb-4">{{ t('aiInput.previewMessage') }}</v-alert>
          <div v-for="(family, familyIndex) in generatedData" :key="familyIndex" class="mb-6 pa-4 border rounded">
            <h4 class="text-h6 mb-2">{{ t('aiInput.family') }} #{{ familyIndex + 1 }}</h4>
            <v-alert v-if="family.validationErrors && family.validationErrors.length" type="warning" class="mb-2">
              <p>{{ t('aiInput.validationErrorsFound') }}</p>
              <ul>
                <li v-for="(error, errorIndex) in family.validationErrors" :key="errorIndex">{{ error }}</li>
              </ul>
            </v-alert>
            <v-divider class="mb-2"></v-divider>
            <div v-for="key in displayKeys" :key="key">
              <p class="text-body-2">
                <strong>{{ t(`family.${key}`) || key }}:</strong>
                {{ formatValue(family[key as keyof Family], key) }}
              </p>
            </div>
          </div>
        </div>
      </v-card-text>
      <v-card-actions>
        <v-spacer></v-spacer>
        <v-btn color="grey-darken-1" @click="cancel" :disabled="loading">{{ t('aiInput.cancelButton')
          }}</v-btn>
        <v-btn color="primary" :disabled="!generatedData || !generatedData.length || loading || hasValidationErrors" @click="save">{{
          t('aiInput.saveButton') }}</v-btn>
      </v-card-actions>
      <v-progress-linear
        v-if="loading"
        indeterminate
        color="primary"
        height="4"
        class="mb-0"
      ></v-progress-linear>
    </v-card>
  </v-dialog>
</template>

<script setup lang="ts">
import { ref, watch, computed } from 'vue';
import { useI18n } from 'vue-i18n';
import { useNaturalLanguageInputStore } from '@/stores/naturalLanguageInput.store';
import { useFamilyStore } from '@/stores/family.store';
import { useNotificationStore } from '@/stores/notification.store';
import type { Family } from '@/types';

const props = defineProps({
  modelValue: Boolean,
});

const emit = defineEmits([
  'update:modelValue',
  'saved',
]);

const { t } = useI18n();
const naturalLanguageInputStore = useNaturalLanguageInputStore();
const familyStore = useFamilyStore();
const notificationStore = useNotificationStore();

const prompt = ref(`Tạo gia đình Nguyễn tại Hà Nội. Gia đình này nổi tiếng về truyền thống học vấn và là dòng dõi học giả. Họ có trụ sở chính ở quận Hoàn Kiếm. Số điện thoại liên hệ là 0123456789, email liên hệ: nguyensfamily@example.com. Trang web chính thức là www.nguyensfamily.vn. Gia đình được thành lập từ năm 1800. Có một huy hiệu gia đình màu xanh với hình sách. Thêm các tag liên quan: học giả, Hà Nội, Việt Nam. Hình ảnh đại diện: https://www.w3schools.com/w3images/avatar5.png. Số thành viên hiện tại là 15, với 4 thế hệ.`);
const generatedData = ref<Family[] | null>(null);
const loading = ref(false);
const form = ref<HTMLFormElement | null>(null);

const displayKeys = [
  'name',
  'description',
  'address',
  'totalMembers',
  'totalGenerations',
  'visibility',
  'avatarUrl',
];

const hasValidationErrors = computed(() => {
  return generatedData.value?.some(family => family.validationErrors && family.validationErrors.length > 0) || false;
});

const formatValue = (value: any, key: string) => {
  if (value === null || value === '') {
    return t('common.unknown');
  }
  if (key === 'visibility') {
    return t(`family.management.visibility.${value.toLowerCase()}`);
  }
  return value;
};

const rules = {
  required: (value: string) => !!value || t('aiInput.promptRequired'),
  length: (len: number) => (value: string) =>
    (value || '').length <= len ||
    t('aiInput.promptLength', { length: len }),
};

watch(() => props.modelValue, (newValue) => {
  if (!newValue) {
    // Reset state when dialog is closed
    prompt.value = '';
    generatedData.value = null;
  }
});

const generateData = async () => {
  if (!form.value) return;
  const { valid } = await form.value.validate();
  if (!valid) return;

  loading.value = true;
  naturalLanguageInputStore.error = null; // Clear previous errors
  try {
    const result = await naturalLanguageInputStore.generateFamilyData(prompt.value);
    if (result) {
      generatedData.value = result;
    } else {
      generatedData.value = null;
      if (naturalLanguageInputStore.error) {
        notificationStore.showSnackbar(naturalLanguageInputStore.error, 'error');
      }
    }
  } catch (error) {
    console.error('Error generating data:', error);
    notificationStore.showSnackbar(
      t('aiInput.generateError', { entity: t('aiInput.families') }),
      'error',
    );
  } finally {
    loading.value = false;
  }
};

const save = async () => {
  if (!generatedData.value || !generatedData.value.length) return;

  try {
    await familyStore.addItems(generatedData.value);
    notificationStore.showSnackbar(
      t('aiInput.saveSuccess'),
      'success',
    );
    emit('saved');
    emit('update:modelValue', false);
  } catch (error) {
    console.error('Error saving generated data:', error);
    notificationStore.showSnackbar(
      t('aiInput.saveError'),
      'error',
    );
  }
};

const cancel = () => {
  naturalLanguageInputStore.error = null; // Clear error on cancel
  emit('update:modelValue', false);
};
</script>