<template>
  <v-form ref="formRef" @submit.prevent>
    <v-row>
      <v-col cols="12">
        <v-text-field
          v-model="form.title"
          :label="t('memoryItem.form.title')"
          :rules="[v => !!v || t('common.validations.required')]"
          required
          data-testid="memory-item-title"
          :readonly="props.readOnly"
        ></v-text-field>
      </v-col>
      <v-col cols="12">
        <v-textarea
          v-model="form.description"
          :label="t('memoryItem.form.description')"
          rows="3"
          data-testid="memory-item-description"
          :readonly="props.readOnly"
        ></v-textarea>
      </v-col>
      <v-col cols="12" md="6">
        <v-text-field
          v-model="happenedAtDate"
          :label="t('memoryItem.form.happenedAt')"
          type="date"
          data-testid="memory-item-happened-at"
          :readonly="props.readOnly"
        ></v-text-field>
      </v-col>
      <v-col cols="12" md="6">
        <v-select
          v-model="form.emotionalTag"
          :items="emotionalTagOptions"
          :label="t('memoryItem.form.emotionalTag')"
          :rules="[v => v !== null && v !== undefined || t('common.validations.required')]"
          required
          data-testid="memory-item-emotional-tag"
          item-title="title"
          item-value="value"
          :readonly="props.readOnly"
        ></v-select>
      </v-col>
    </v-row>

    <!-- Memory Media Section -->
    <v-card class="mb-4" :elevation="1">
      <v-card-title>{{ t('memoryItem.form.mediaSectionTitle') }}</v-card-title>
      <v-card-text>
        <div v-for="(media, index) in form.media" :key="index" class="mb-2">
          <v-row align="center">
            <v-col cols="4">
              <v-select
                v-model="media.mediaType"
                :items="mediaTypeOptions"
                :label="t('memoryItem.form.mediaType')"
                item-title="title"
                item-value="value"
                :readonly="props.readOnly"
              ></v-select>
            </v-col>
            <v-col cols="6">
              <v-text-field
                v-model="media.url"
                :label="t('memoryItem.form.mediaUrl')"
                :rules="[v => !!v || t('common.validations.required'), v => (v && v.startsWith('http')) || t('common.validations.validUrl')]"
                :readonly="props.readOnly"
              ></v-text-field>
            </v-col>
            <v-col cols="2" v-if="!props.readOnly">
              <v-btn icon flat color="error" @click="removeMedia(index)">
                <v-icon>mdi-delete</v-icon>
              </v-btn>
            </v-col>
          </v-row>
        </div>
        <v-btn v-if="!props.readOnly" color="primary" @click="addMedia">
          {{ t('memoryItem.form.addMedia') }}
        </v-btn>
      </v-card-text>
    </v-card>

    <!-- Memory Persons Section -->
    <v-card :elevation="1">
      <v-card-title>{{ t('memoryItem.form.personsSectionTitle') }}</v-card-title>
      <v-card-text>
        <div v-for="(person, index) in form.persons" :key="index" class="mb-2">
          <v-row align="center">
            <v-col cols="10">
              <v-text-field
                v-model="person.memberId"
                :label="t('memoryItem.form.memberId')"
                :rules="[v => !!v || t('common.validations.required')]"
                :readonly="props.readOnly"
              ></v-text-field>
            </v-col>
            <v-col cols="2" v-if="!props.readOnly">
              <v-btn icon flat color="error" @click="removePerson(index)">
                <v-icon>mdi-delete</v-icon>
              </v-btn>
            </v-col>
          </v-row>
        </div>
        <v-btn v-if="!props.readOnly" color="primary" @click="addPerson">
          {{ t('memoryItem.form.addPerson') }}
        </v-btn>
      </v-card-text>
    </v-card>
  </v-form>
</template>

<script setup lang="ts">
import { ref, computed, watch, reactive } from 'vue';
import { useI18n } from 'vue-i18n';
import type { VForm } from 'vuetify/components';
import type { MemoryItem, MemoryMedia, MemoryPerson } from '@/types';
import { EmotionalTag, MediaType } from '@/types/enums';

interface MemoryItemFormProps {
  initialMemoryItemData?: MemoryItem;
  familyId: string;
  readOnly?: boolean;
}

const props = defineProps<MemoryItemFormProps>();

const formRef = ref<VForm | null>(null);
const { t } = useI18n();

const defaultNewMemoryItem: MemoryItem = {
  id: '',
  familyId: props.familyId,
  title: '',
  description: undefined,
  happenedAt: undefined,
  emotionalTag: EmotionalTag.Neutral,
  media: [],
  persons: [],
  created: new Date(),
  createdBy: undefined,
  lastModified: undefined,
  lastModifiedBy: undefined,
};

const form = reactive<MemoryItem>(
  props.initialMemoryItemData ? { ...props.initialMemoryItemData } : { ...defaultNewMemoryItem },
);

const happenedAtDate = computed({
  get: () => form.happenedAt ? new Date(form.happenedAt).toISOString().split('T')[0] : '',
  set: (val: string) => {
    if (val) {
      form.happenedAt = new Date(val);
    } else {
      form.happenedAt = undefined;
    }
  }
});

watch(() => props.initialMemoryItemData, (newData) => {
  if (newData) {
    Object.assign(form, newData);
  } else {
    Object.assign(form, { ...defaultNewMemoryItem });
  }
});

const emotionalTagOptions = computed(() => [
  { title: t('memoryItem.emotionalTag.happy'), value: EmotionalTag.Happy },
  { title: t('memoryItem.emotionalTag.sad'), value: EmotionalTag.Sad },
  { title: t('memoryItem.emotionalTag.proud'), value: EmotionalTag.Proud },
  { title: t('memoryItem.emotionalTag.memorial'), value: EmotionalTag.Memorial },
  { title: t('memoryItem.emotionalTag.neutral'), value: EmotionalTag.Neutral },
]);

const mediaTypeOptions = computed(() => [
  { title: t('common.mediaType.Image'), value: MediaType.Image },
  { title: t('common.mediaType.Video'), value: MediaType.Video },
  { title: t('common.mediaType.Audio'), value: MediaType.Audio },
  { title: t('common.mediaType.Document'), value: MediaType.Document },
]);

const addMedia = () => {
  form.media.push({ id: '', memoryItemId: '', mediaType: MediaType.Image, url: '' });
};

const removeMedia = (index: number) => {
  form.media.splice(index, 1);
};

const addPerson = () => {
  form.persons.push({ memberId: '' });
};

const removePerson = (index: number) => {
  form.persons.splice(index, 1);
};

const validate = async () => {
  if (!formRef.value) return false;
  const { valid } = await formRef.value.validate();
  return valid;
};

const getFormData = (): MemoryItem => {
  return { ...form };
};

defineExpose({
  validate,
  getFormData,
});
</script>
