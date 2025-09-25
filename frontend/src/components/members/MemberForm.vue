<template>
  <v-card>
    <v-card-title class="text-center">
      <span class="text-h5 text-uppercase">{{ title }}</span>
    </v-card-title>
    <v-card-text>
      <v-tabs v-model="tab">
        <v-tab value="general">{{ t('member.form.tab.general') }}</v-tab>
        <v-tab value="timeline">{{ t('member.form.tab.timeline') }}</v-tab>
      </v-tabs>

      <v-window v-model="tab">
        <v-window-item value="general">
          <v-form
            ref="form"
            @submit.prevent="submitForm"
            :disabled="props.readOnly"
          >
            <!-- Thông tin cơ bản -->
            <v-row>
              <v-col cols="12">
                <div class="d-flex justify-center mb-4">
                  <v-avatar size="96">
                    <v-img
                      v-if="memberForm.avatarUrl"
                      :src="memberForm.avatarUrl"
                    ></v-img>
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
                  v-model="memberForm.lastName"
                  :label="t('member.form.lastName')"
                  :rules="[rules.required]"
                  :readonly="props.readOnly"
                ></v-text-field>
              </v-col>
              <v-col cols="12" md="6">
                <v-text-field
                  v-model="memberForm.firstName"
                  :label="t('member.form.firstName')"
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
                <v-autocomplete
                  v-model="memberForm.familyId"
                  :items="familyStore.items"
                  item-value="id"
                  :label="t('member.form.familyId')"
                  :rules="[rules.required]"
                  :readonly="props.readOnly"
                  clearable
                  :item-props="formatFamilyItemProps"
                >
                  <template v-slot:selection="{ item }">
                    <v-chip
                      v-if="item.raw"
                      :prepend-avatar="item.raw.avatarUrl"
                      :closable="!props.readOnly"
                      @click:close="memberForm.familyId = ''"
                    >
                      {{ item.raw.name }}
                    </v-chip>
                  </template>
                  <template v-slot:item="{ props, item }">
                    <AutocompleteItem v-bind="props" :item="item.raw" />
                  </template>
                </v-autocomplete>
              </v-col>
            </v-row>
            <v-row>
              <v-col cols="12" md="4">
                <v-autocomplete
                  v-model="memberForm.fatherId"
                  :items="potentialFathers"
                  item-title="fullName"
                  item-value="id"
                  :label="t('member.form.father')"
                  :readonly="props.readOnly"
                  clearable
                >
                  <template v-slot:item="{ props, item }">
                    <AutocompleteItem v-bind="props" :item="item.raw" />
                  </template>
                </v-autocomplete>
              </v-col>
              <v-col cols="12" md="4">
                <v-autocomplete
                  v-model="memberForm.motherId"
                  :items="potentialMothers"
                  item-title="fullName"
                  item-value="id"
                  :label="t('member.form.mother')"
                  :readonly="props.readOnly"
                  clearable
                >
                  <template v-slot:item="{ props, item }">
                    <AutocompleteItem v-bind="props" :item="item.raw" />
                  </template>
                </v-autocomplete>
              </v-col>
              <v-col cols="12" md="4">
                <v-autocomplete
                  v-model="memberForm.spouseId"
                  :items="potentialSpouses"
                  item-title="fullName"
                  item-value="id"
                  :label="t('member.form.spouse')"
                  :readonly="props.readOnly"
                  clearable
                >
                  <template v-slot:item="{ props, item }">
                    <AutocompleteItem v-bind="props" :item="item.raw" />
                  </template>
                </v-autocomplete>
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
      <v-btn color="blue-darken-1" variant="text" @click="closeForm">{{
        props.readOnly ? t('common.close') : t('common.cancel')
      }}</v-btn>
      <v-btn
        v-if="!props.readOnly"
        color="blue-darken-1"
        variant="text"
        @click="submitForm"
        >{{ t('common.save') }}</v-btn
      >
    </v-card-actions>
  </v-card>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue';
import type { Member } from '@/types/family';
import { useI18n } from 'vue-i18n';
import {
  DateInputField,
  GenderSelect,
  AutocompleteItem,
} from '@/components/common';
import MemberTimeline from './MemberTimeline.vue';
import { useFamilyStore } from '@/stores/family.store';
import { useMemberStore } from '@/stores/member.store';

import type { TimelineEvent } from '@/types/timeline/timeline-event';

const props = defineProps<{
  readOnly?: boolean;
  initialMemberData?: Member;
  title: string;
}>();

const emit = defineEmits(['close', 'submit']);

const { t } = useI18n();
const familyStore = useFamilyStore();
const memberStore = useMemberStore();

onMounted(() => {
  familyStore.fetchAllItems();
});

const tab = ref('general');

const form = ref<HTMLFormElement | null>(null);
const timelineEvents = ref<TimelineEvent[]>([]);

// Mock data for timeline
const mockTimelineEvents = [
  {
    year: 2005,
    title: 'Tốt nghiệp cấp 3',
    description:
      'Hoàn thành chương trình trung học phổ thông với bằng danh dự.',
    color: 'blue',
  },
  {
    year: 2009,
    title: 'Tốt nghiệp đại học',
    description: 'Nhận bằng Cử nhân Khoa học Máy tính.',
    color: 'blue',
  },
  {
    year: 2010,
    title: 'Bắt đầu công việc đầu tiên',
    description:
      'Bắt đầu sự nghiệp với tư cách là một nhà phát triển phần mềm.',
    color: 'green',
  },
  {
    year: 2015,
    title: 'Kết hôn',
    description: 'Kết hôn với người bạn đời của mình.',
    color: 'pink',
  },
  {
    year: 2018,
    title: 'Sinh con đầu lòng',
    description: 'Chào đón đứa con đầu lòng chào đời.',
    color: 'purple',
  },
];
timelineEvents.value = mockTimelineEvents;

const memberForm = ref<Omit<Member, 'id'> | Member>(
  props.initialMemberData
    ? {
        ...props.initialMemberData,
        fatherId:
          props.initialMemberData.fatherId === undefined
            ? null
            : props.initialMemberData.fatherId,
        motherId:
          props.initialMemberData.motherId === undefined
            ? null
            : props.initialMemberData.motherId,
        spouseId:
          props.initialMemberData.spouseId === undefined
            ? null
            : props.initialMemberData.spouseId,
      }
    : {
        lastName: '',
        firstName: '',
        dateOfBirth: undefined,
        gender: 'male',
        familyId: '',
        fatherId: null,
        motherId: null,
        spouseId: null,
      },
);

const membersInFamily = computed(() => {
  if (!memberForm.value.familyId || !memberStore.items) {
    return [];
  }
  return memberStore.items.filter(
    (m) => m.familyId === memberForm.value.familyId,
  );
});

const potentialFathers = computed(() => {
  return membersInFamily.value.filter(
    (m) => m.gender === 'male' && m.id !== props.initialMemberData?.id,
  );
});

const potentialMothers = computed(() => {
  return membersInFamily.value.filter(
    (m) => m.gender === 'female' && m.id !== props.initialMemberData?.id,
  );
});

const potentialSpouses = computed(() => {
  return membersInFamily.value.filter(
    (m) => m.id !== props.initialMemberData?.id,
  );
});

const formatFamilyItemProps = (item: any) => {
  return {
    title: item.name,
    subtitle: item.address,
  };
};

const rules = {
  required: (value: unknown) => !!value || t('validation.required'),
};

const dateOfDeathRule = (value: string | Date | null) => {
  if (!value) return true;
  if (!memberForm.value.dateOfBirth) return true;

  const dateOfDeath = typeof value === 'string' ? new Date(value) : value;
  const dateOfBirth =
    typeof memberForm.value.dateOfBirth === 'string'
      ? new Date(memberForm.value.dateOfBirth)
      : memberForm.value.dateOfBirth;

  if (!dateOfDeath || !dateOfBirth) return true;
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
