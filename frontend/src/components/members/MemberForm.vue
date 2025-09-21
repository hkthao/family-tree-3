<template>
  <v-card>
    <v-card-title class="text-center">
      <span class="text-h5 text-uppercase">{{ title }}</span>
    </v-card-title>
    <v-card-text>
      <v-tabs v-model="tab" >
        <v-tab value="general">{{ t('member.form.tab.general') }}</v-tab>
        <v-tab value="timeline">{{ t('member.form.tab.timeline') }}</v-tab>
      </v-tabs>

      <v-window v-model="tab">
        <v-window-item value="general">
          <v-form ref="form" @submit.prevent="submitForm" :disabled="props.readOnly">
            <!-- Thông tin cơ bản -->
            <v-row>
              <v-col cols="12">
                <div class="d-flex justify-center mb-4">
                  <v-avatar size="96">
                    <v-img v-if="memberForm.avatarUrl" :src="memberForm.avatarUrl"></v-img>
                    <v-icon v-else size="96">mdi-account-circle</v-icon>
                  </v-avatar>
                </div>
                <v-text-field
                  v-model="memberForm.avatarUrl"
                  :label="t('member.form.avatarUrl')"
                  :readonly="props.readOnly"
                ></v-text-field>
              </v-col>
            </v-row>
            <v-row>
              <v-col cols="12" md="6">
                <v-text-field
                  v-model="memberForm.fullName"
                  :label="t('member.form.fullName')"
                  :rules="[rules.required]"
                  :readonly="props.readOnly"
                ></v-text-field>
              </v-col>
              <v-col cols="12" md="6">
                <v-text-field
                  v-model="memberForm.nickname"
                  :label="t('member.form.nickname')"
                  :readonly="props.readOnly"
                ></v-text-field>
              </v-col>
            </v-row>
            <v-row>
              <v-col cols="12" md="6">
                <DateInputField
                  v-model="memberForm.dateOfBirth"
                  :label="t('member.form.dateOfBirth')"
                  :rules="[rules.required]"
                  :readonly="props.readOnly"
                />
              </v-col>
              <v-col cols="12" md="6">
                <DateInputField
                  v-model="memberForm.dateOfDeath"
                  :label="t('member.form.dateOfDeath')"
                  optional
                  :readonly="props.readOnly"
                  :rules="[dateOfDeathRule]"
                />
              </v-col>
            </v-row>

            <!-- Thông tin cá nhân -->
            <v-row>
              <v-col cols="12" md="4">
                <GenderSelect
                  v-model="memberForm.gender"
                  :label="t('member.form.gender')"
                  :rules="[rules.required]"
                  :read-only="props.readOnly"
                />
              </v-col>
              <v-col cols="12" md="4">
                <v-text-field
                  v-model="memberForm.placeOfBirth"
                  :label="t('member.form.placeOfBirth')"
                  :readonly="props.readOnly"
                ></v-text-field>
              </v-col>
              <v-col cols="12" md="4">
                <v-text-field
                  v-model="memberForm.placeOfDeath"
                  :label="t('member.form.placeOfDeath')"
                  :readonly="props.readOnly"
                ></v-text-field>
              </v-col>
            </v-row>
            <v-row>
              <v-col cols="12">
                <v-text-field
                  v-model="memberForm.occupation"
                  :label="t('member.form.occupation')"
                  :readonly="props.readOnly"
                ></v-text-field>
              </v-col>
            </v-row>

            <v-row>
              <v-col cols="12">
                <FamilyAutocomplete
                  v-model="memberForm.familyId"
                  :label="t('member.form.familyId')"
                  :rules="[rules.required]"
                  :read-only="props.readOnly"
                />
              </v-col>
            </v-row>
            <v-row>
              <v-col cols="12" md="4">
                <v-autocomplete
                  v-model="memberForm.fatherId"
                  :label="t('member.form.father')"
                  :items="[]"
                  item-title="fullName"
                  item-value="id"
                  :readonly="props.readOnly"
                />
              </v-col>
              <v-col cols="12" md="4">
                <v-autocomplete
                  v-model="memberForm.motherId"
                  :label="t('member.form.mother')"
                  :items="[]"
                  item-title="fullName"
                  item-value="id"
                  :readonly="props.readOnly"
                />
              </v-col>
              <v-col cols="12" md="4">
                <v-autocomplete
                  v-model="memberForm.spouseId"
                  :label="t('member.form.spouse')"
                  :items="[]"
                  item-title="fullName"
                  item-value="id"
                  :readonly="props.readOnly"
                />
              </v-col>
            </v-row>

            <!-- Thông tin khác -->
            <v-row>
              <v-col cols="12">
                <v-textarea
                  v-model="memberForm.biography"
                  :label="t('member.form.biography')"
                  :readonly="props.readOnly"
                ></v-textarea>
              </v-col>
            </v-row>
          </v-form>
        </v-window-item>

        <v-window-item value="timeline">
          <MemberTimeline
            :timeline-events="timelineEvents"
            :read-only="props.readOnly"
            @add="handleAddTimelineEvent"
            @edit="handleEditTimelineEvent"
            @delete="handleDeleteTimelineEvent"
          />
        </v-window-item>
      </v-window>
    </v-card-text>
    <v-card-actions>
      <v-spacer></v-spacer>
      <v-btn color="blue-darken-1" variant="text" @click="closeForm">{{ props.readOnly ? t('common.close') : t('common.cancel') }}</v-btn>
      <v-btn v-if="!props.readOnly" color="blue-darken-1" variant="text" @click="submitForm">{{ t('common.save') }}</v-btn>
    </v-card-actions>
  </v-card>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import type { Member } from '@/services/member.service';
import { useI18n } from 'vue-i18n';
import DateInputField from '@/components/common/DateInputField.vue';
import MemberTimeline from '@/components/members/MemberTimeline.vue';
import FamilyAutocomplete from '@/components/common/FamilyAutocomplete.vue';
import GenderSelect from '@/components/common/GenderSelect.vue';

interface TimelineEvent {
  year: string;
  title: string;
  description: string;
  color: string;
}

const props = defineProps<{
  readOnly?: boolean;
  initialMemberData?: Member;
  title: string;
}>();

const emit = defineEmits(['close', 'submit']);

const { t } = useI18n();

const tab = ref('general'); // Default to general tab

const form = ref<HTMLFormElement | null>(null);
const memberForm = ref<Omit<Member, 'id'> | Member>(props.initialMemberData || {
  fullName: '',
  dateOfBirth: null,
  gender: 'Male',
  familyId: '', // Add familyId
});

const rules = {
  required: (value: string) => !!value || t('validation.required'),
};

const dateOfDeathRule = (value: Date | null) => {
  if (!value) return true; // Optional field
  if (!memberForm.value.dateOfBirth) return true; // Cannot compare if dateOfBirth is not set

  const dateOfDeath = new Date(value);
  const dateOfBirth = new Date(memberForm.value.dateOfBirth);

  return dateOfDeath > dateOfBirth || t('validation.dateOfDeathAfterBirth');
};







const submitForm = async () => {
  if (form.value) {
    const { valid } = await form.value.validate();
    if (valid) {
      emit('submit', memberForm.value);
    }
  }
};

const closeForm = () => {
  emit('close');
};

// Placeholder functions for timeline events
const handleAddTimelineEvent = () => {
  console.log('Add timeline event');
};

const handleEditTimelineEvent = (event: TimelineEvent) => {
  console.log('Edit timeline event:', event);
};

const handleDeleteTimelineEvent = (event: TimelineEvent) => {
  console.log('Delete timeline event:', event);
};
</script>