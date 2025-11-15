<template>
  <v-data-table-server v-model:items-per-page="itemsPerPage" :headers="headers" :items="items"
    :items-length="totalItems" :loading="loading" item-value="id" @update:options="loadMembers" elevation="0"
    data-testid="member-list" fixed-header>
    <template #top>
      <v-toolbar flat>
        <v-toolbar-title>{{ t('member.list.title') }}</v-toolbar-title>
        <v-spacer></v-spacer>
        <v-btn v-if="canPerformActions" color="primary" icon @click="$emit('ai-create')">
          <v-tooltip :text="t('member.list.action.aiCreate')">
            <template v-slot:activator="{ props }">
              <v-icon v-bind="props">mdi-robot-happy-outline</v-icon>
            </template>
          </v-tooltip>
        </v-btn>
        <v-btn v-if="canPerformActions" color="primary" icon @click="$emit('create')"
          data-testid="add-new-member-button">
          <v-tooltip :text="t('member.list.action.create')">
            <template v-slot:activator="{ props }">
              <v-icon v-bind="props">mdi-plus</v-icon>
            </template>
          </v-tooltip>
        </v-btn>
        <v-text-field v-model="debouncedSearch" :label="t('common.search')" append-inner-icon="mdi-magnify" single-line
          hide-details clearable class="mr-2" data-test-id="member-list-search-input"></v-text-field>
      </v-toolbar>
    </template>
    <!-- Avatar column -->
    <template #item.avatarUrl="{ item }">
      <div class="d-flex justify-center">
        <v-avatar size="36" class="my-2">
          <v-img :src="getMemberAvatar(item)" :alt="item.fullName" />
        </v-avatar>
      </div>
    </template>

    <!-- Full Name column -->
    <template #item.fullName="{ item }">
      <a @click="viewMember(item)" class="text-primary font-weight-bold text-decoration-underline cursor-pointer">
        {{ item.fullName }}
      </a>
      <div class="text-caption text-medium-emphasis">
        {{ item.code }}
      </div>
    </template>

    <!-- Father column -->
    <template #item.father="{ item }">
      <MemberName :full-name="item.fatherFullName" :avatar-url="item.fatherAvatarUrl" :gender="item.fatherGender" />
    </template>

    <!-- Mother column -->
    <template #item.mother="{ item }">
      <MemberName :full-name="item.motherFullName" :avatar-url="item.motherAvatarUrl" :gender="item.motherGender" />
    </template>

    <!-- Spouse column -->
    <template #item.spouse="{ item }">
      <MemberName v-if="item.husbandFullName" :full-name="item.husbandFullName" :avatar-url="item.husbandAvatarUrl" :gender="item.husbandGender" />
      <MemberName v-if="item.wifeFullName" :full-name="item.wifeFullName" :avatar-url="item.wifeAvatarUrl" :gender="item.wifeGender" />
    </template>

    <!-- Family column -->
    <template #item.family="{ item }">
      <ChipLookup :modelValue="item.familyId" :data-source="familyStore" display-expr="name" value-expr="id"
        imageExpr="avatarUrl" />
    </template>

    <!-- Birth/Death Years column -->
    <template #item.birthDeathYears="{ item }">
      {{ item.birthDeathYears }}
    </template>

    <!-- Gender column -->
    <template #item.gender="{ item }">
      <v-chip label size="small" class="text-capitalize">
        {{ getGenderTitle(item.gender) }}
      </v-chip>
    </template>

    <!-- Actions column -->
    <template #item.actions="{ item }">
      <div v-if="canPerformActions">
        <v-menu>
          <template v-slot:activator="{ props }">
            <v-btn icon variant="text" v-bind="props" size="small">
              <v-icon>mdi-dots-vertical</v-icon>
            </v-btn>
          </template>
          <v-list>
            <v-list-item @click="$emit('ai-biography', item)">
              <v-list-item-title>{{ t('member.list.action.aiBiography') }}</v-list-item-title>
            </v-list-item>
            <v-list-item @click="editMember(item)" data-testid="edit-member-button">
              <v-list-item-title>{{ t('member.list.action.edit') }}</v-list-item-title>
            </v-list-item>
            <v-list-item @click="confirmDelete(item)" data-testid="delete-member-button">
              <v-list-item-title>{{ t('member.list.action.delete') }}</v-list-item-title>
            </v-list-item>
          </v-list>
        </v-menu>
      </div>
    </template>

    <!-- Loading state -->
    <template #loading>
      <v-skeleton-loader type="table-row@5" data-testid="member-list-loading" />
    </template>
  </v-data-table-server>
</template>

<script setup lang="ts">
import { ref, computed, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import type { Member } from '@/types';
import type { DataTableHeader } from 'vuetify';
import { useFamilyStore } from '@/stores/family.store';
import { ChipLookup } from '@/components/common';
import { MemberName } from '@/components/member';
import { getGenderTitle } from '@/constants/genders';
import { useAuth } from '@/composables/useAuth';
import maleAvatar from '@/assets/images/male_avatar.png';
import femaleAvatar from '@/assets/images/female_avatar.png';
import { DEFAULT_ITEMS_PER_PAGE } from '@/constants/pagination';

const familyStore = useFamilyStore();
const { isAdmin, isFamilyManager } = useAuth();

const getMemberAvatar = (member: Member) => {
  if (member.avatarUrl) {
    return member.avatarUrl;
  }
  if (member.gender === 'Male') {
    return maleAvatar;
  }
  if (member.gender === 'Female') {
    return femaleAvatar;
  }
  return maleAvatar; // Fallback for 'Other' or undefined gender
};

const props = defineProps<{
  items: Member[];
  totalItems: number;
  loading: boolean;
  search: string;
  readOnly?: boolean; // Add readOnly prop
}>();

const canPerformActions = computed(() => {
  return !props.readOnly && (isAdmin.value || isFamilyManager.value);
});

const emit = defineEmits([
  'update:options',
  'view',
  'edit',
  'delete',
  'create',
  'ai-biography',
  'ai-create',
  'update:search',
]);

const { t } = useI18n();

const searchQuery = ref(props.search);
let debounceTimer: ReturnType<typeof setTimeout>;

const debouncedSearch = computed({
  get() {
    return searchQuery.value;
  },
  set(newValue: string) {
    searchQuery.value = newValue;
    clearTimeout(debounceTimer);
    debounceTimer = setTimeout(() => {
      emit('update:search', newValue);
    }, 300);
  },
});

watch(() => props.search, (newSearch) => {
  if (newSearch !== searchQuery.value) {
    searchQuery.value = newSearch;
  }
});

// ... (rest of the file)

const itemsPerPage = ref(DEFAULT_ITEMS_PER_PAGE);

const headers = computed<DataTableHeader[]>(() => {
  const baseHeaders: DataTableHeader[] = [
    {
      title: t('member.list.headers.avatar'),
      key: 'avatarUrl',
      sortable: false,
      width: '100px',
      align: 'center',
    },
    {
      title: t('member.list.headers.fullName'),
      key: 'fullName',
      width: '250px',
      align: 'start',
    },
    {
      title: t('member.form.father'),
      key: 'father',
      width: 'auto',
      align: 'start',
      sortable: false,
    },
    {
      title: t('member.form.mother'),
      key: 'mother',
      width: 'auto',
      align: 'start',
      sortable: false,
    },
    {
      title: t('member.form.spouse'),
      key: 'spouse',
      width: 'auto',
      align: 'start',
      sortable: false,
    },
    {
      title: t('member.list.headers.birthDeathYears'),
      key: 'birthDeathYears',
      width: '120px',
      align: 'center',
      sortable: false,
    },
    {
      title: t('member.list.headers.gender'),
      key: 'gender',
      width: '110px',
      align: 'center',
    },
  ];

  if (canPerformActions.value) {
    baseHeaders.push({
      title: t('member.list.headers.actions'),
      key: 'actions',
      sortable: false,
      align: 'end',
    });
  }
  return baseHeaders;
});

const loadMembers = (options: {
  page: number;
  itemsPerPage: number;
  sortBy: { key: string; order: string }[]; // Corrected type
}) => {
  emit('update:options', options);
};

const viewMember = (member: Member) => {
  emit('view', member);
};

const editMember = (member: Member) => {
  emit('edit', member);
};

const confirmDelete = (member: Member) => {
  emit('delete', member);
};
</script>
