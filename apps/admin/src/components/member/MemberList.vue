<template>
  <v-data-table-server v-model:items-per-page="itemsPerPage" :headers="headers" :items="items"
    :items-length="totalItems" :loading="loading" item-value="id" @update:options="loadMembers" elevation="0"
    data-testid="member-list" fixed-header>
    <template #top>
      <v-toolbar flat>
        <v-toolbar-title>{{ t('member.list.title') }}</v-toolbar-title>
        <v-spacer></v-spacer>
        <v-btn v-if="props.allowAdd" color="primary" icon @click="$emit('ai-create')">
          <v-tooltip :text="t('member.list.action.aiCreate')">
            <template v-slot:activator="{ props }">
              <v-icon v-bind="props">mdi-robot-happy-outline</v-icon>
            </template>
          </v-tooltip>
        </v-btn>
        <v-btn v-if="props.allowAdd" color="primary" icon @click="$emit('create')"
          data-testid="add-new-member-button">
          <v-tooltip :text="t('member.list.action.create')">
            <template v-slot:activator="{ props }">
              <v-icon v-bind="props">mdi-plus</v-icon>
            </template>
          </v-tooltip>
        </v-btn>
        <v-text-field v-model="searchQuery" :label="t('common.search')" append-inner-icon="mdi-magnify" single-line
          hide-details clearable class="mr-2" data-test-id="member-list-search-input"></v-text-field>
      </v-toolbar>
    </template>
    <!-- Avatar column -->
    <template #item.avatarUrl="{ item }">
      <MemberAvatarDisplay :member="item" />
    </template>

    <!-- Full Name column -->
    <template #item.fullName="{ item }">
      <div class="member-full-name-column">
<a @click="viewMember(item)" class="text-primary font-weight-bold text-decoration-underline cursor-pointer">
        {{ item.fullName }}
      </a>
      <div class="text-caption text-medium-emphasis">
        {{ item.code }}
      </div>
      </div>
      
    </template>

    <!-- Father column -->
    <template #item.father="{ item }">
      <MemberName :full-name="item.fatherFullName" :avatar-url="item.fatherAvatarUrl" :gender="Gender.Male" />
    </template>

    <!-- Mother column -->
    <template #item.mother="{ item }">
      <MemberName :full-name="item.motherFullName" :avatar-url="item.motherAvatarUrl" :gender="Gender.Female" />
    </template>

    <!-- Spouse column -->
    <template #item.spouse="{ item }">
      <MemberName v-if="item.husbandFullName" :full-name="item.husbandFullName" :avatar-url="item.husbandAvatarUrl" :gender="Gender.Male" />
      <MemberName v-if="item.wifeFullName" :full-name="item.wifeFullName" :avatar-url="item.wifeAvatarUrl" :gender="Gender.Female" />
    </template>

    <!-- Family column -->
    <template #item.family="{ item }">
      <FamilyName :name="item.familyName" :avatar-url="item.familyAvatarUrl" />
    </template>

    <!-- Birth/Death Years column -->
    <template #item.birthDeathYears="{ item }">
      {{ item.birthDeathYears }}
    </template>

    <!-- Gender column -->
    <template #item.gender="{ item }">
      <MemberGenderChip :gender="item.gender" />
    </template>

    <!-- Actions column -->
    <template #item.actions="{ item }">
      <div class="d-flex ga-2" v-if="props.allowEdit || props.allowDelete">
        <v-tooltip :text="t('member.list.action.edit')">
          <template v-slot:activator="{ props: tooltipProps }">
            <v-btn icon size="small" variant="text" v-bind="tooltipProps" @click="editMember(item)"
              data-testid="edit-member-button" aria-label="Edit" v-if="props.allowEdit">
              <v-icon>mdi-pencil</v-icon>
            </v-btn>
          </template>
        </v-tooltip>
        <v-tooltip :text="t('member.list.action.delete')">
          <template v-slot:activator="{ props: tooltipProps }">
            <v-btn icon size="small" variant="text" v-bind="tooltipProps" @click="confirmDelete(item)"
              data-testid="delete-member-button" :data-member-name="item.fullName" aria-label="Delete" v-if="props.allowDelete">
              <v-icon>mdi-delete</v-icon>
            </v-btn>
          </template>
        </v-tooltip>
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
import { Gender, type Member } from '@/types';
import type { DataTableHeader } from 'vuetify';
import FamilyName from '@/components/common/FamilyName.vue';
import { MemberName, MemberAvatarDisplay, MemberGenderChip } from '@/components/member'; 
import { DEFAULT_ITEMS_PER_PAGE } from '@/constants/pagination';
import { useDebouncedSearch } from '@/composables/family/logic/useDebouncedSearch';

const props = defineProps<{
  items: Member[];
  totalItems: number;
  loading: boolean;
  search?: string; 
  readOnly?: boolean; 
  allowAdd?: boolean;
  allowEdit?: boolean;
  allowDelete?: boolean;
}>();

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

const { state: { searchQuery, debouncedSearchQuery } } = useDebouncedSearch(props.search);

watch(debouncedSearchQuery, (newValue) => {
  emit('update:search', newValue);
});

watch(() => props.search, (newSearch) => {
  if (newSearch !== searchQuery.value) {
    searchQuery.value = newSearch ?? '';
  }
});

const itemsPerPage = ref(DEFAULT_ITEMS_PER_PAGE);

const headers = computed<DataTableHeader[]>(() => {
  const baseHeaders: DataTableHeader[] = [
    {
      title: t('member.list.headers.avatar'),
      key: 'avatarUrl',
      sortable: false,
      minWidth: '110px',
      align: 'center',
    },
    {
      title: t('member.list.headers.fullName'),
      key: 'fullName',
      minWidth: '250px',
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
      minWidth: '120px',
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

  if (props.allowEdit || props.allowDelete) {
    baseHeaders.push({
      title: t('member.list.headers.actions'),
      key: 'actions',
      sortable: false,
      align: 'center',
      minWidth: '120px',
      fixed: "end"
    });
  }
  return baseHeaders;
});

const loadMembers = (options: {
  page: number;
  itemsPerPage: number;
  sortBy: { key: string; order: string }[]; 
}) => {
  emit('update:options', options);
};

const viewMember = (member: Member) => {
  emit('view', member.id);
};

const editMember = (member: Member) => {
  emit('edit', member.id);
};

const confirmDelete = (member: Member) => {
  emit('delete', member.id);
};
</script>
