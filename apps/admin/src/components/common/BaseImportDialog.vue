<template>
  <v-dialog :model-value="modelValue" @update:model-value="emit('update:modelValue', $event)" max-width="500px">
    <v-card>
      <v-card-title>{{ title }}</v-card-title>
      <v-card-text>
        <v-file-input
          v-model="internalFile"
          :label="label"
          :accept="accept"
          prepend-icon="mdi-paperclip"
          @change="onFileChange"
        ></v-file-input>
        <v-alert v-if="internalFile && internalFile.type !== 'application/json'" type="warning" class="mt-2">
          {{ $t('familyLocation.import.invalidFileType') }}
        </v-alert>
        <v-alert v-if="internalFile && maxFileSize && internalFile.size > maxFileSize" type="warning" class="mt-2">
          {{ $t('familyLocation.import.fileTooLarge') }}
        </v-alert>
      </v-card-text>
      <v-card-actions>
        <v-spacer></v-spacer>
        <v-btn color="grey" @click="emit('update:modelValue', false)">{{ $t('common.cancel') }}</v-btn>
        <v-btn
          color="primary"
          :loading="loading"
          :disabled="!!(disabled || !internalFile || (internalFile && internalFile.type !== 'application/json') || (maxFileSize && internalFile && internalFile.size > maxFileSize))"
          @click="emit('import', internalFile)"
        >
          {{ $t('common.import') }}
        </v-btn>
      </v-card-actions>
    </v-card>
  </v-dialog>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue';

interface BaseImportDialogProps {
  modelValue: boolean;
  title: string;
  label: string;
  loading?: boolean;
  disabled?: boolean;
  accept?: string;
  maxFileSize?: number; // in bytes, e.g., 5 * 1024 * 1024 for 5MB
}

const props = withDefaults(defineProps<BaseImportDialogProps>(), {
  loading: false,
  disabled: false,
  accept: '.json',
  maxFileSize: 5 * 1024 * 1024, // Default to 5MB
});

const emit = defineEmits(['update:modelValue', 'import', 'file-change']);

const internalFile = ref<File | null>(null);

const onFileChange = (event: Event) => {
  const target = event.target as HTMLInputElement;
  if (target.files && target.files.length > 0) {
    internalFile.value = target.files[0];
  } else {
    internalFile.value = null;
  }
  emit('file-change', internalFile.value);
};

// Clear internalFile when dialog closes
watch(() => props.modelValue, (newVal) => {
  if (!newVal) {
    internalFile.value = null;
  }
});
</script>