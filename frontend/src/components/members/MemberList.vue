<template>
  <v-data-table-server
    v-model:items-per-page="itemsPerPage"
    :headers="headers"
    :items="members"
    :items-length="totalMembers"
    :loading="loading"
    item-value="id"
    @update:options="loadMembers"
    elevation="0"
  >
    <template #top>
      <v-toolbar flat>
        <v-toolbar-title>{{ t('member.list.title') }}</v-toolbar-title>
        <v-spacer></v-spacer>
        <v-btn color="primary" icon @click="$emit('create')">
          <v-icon>mdi-plus</v-icon>
        </v-btn>
      </v-toolbar>
    </template>
    <!-- Avatar column -->
    <template #item.avatarUrl="{ item }">
      <div class="d-flex justify-center">
        <v-avatar size="36" class="my-2">
          <v-img v-if="item.avatarUrl" :src="item.avatarUrl" :alt="item.fullName" />
          <v-icon v-else>mdi-account-circle</v-icon>
        </v-avatar>
      </div>
    </template>

    <!-- Full Name column -->
    <template #item.fullName="{ item }">
      <div class="text-left">
        <v-btn variant="text" color="primary" @click.prevent="viewMember(item)" class="text-none">
          {{ item.fullName }}
        </v-btn>
      </div>
    </template>

    <!-- Family column -->
    <template #item.family="{ item }">
      {{ getFamilyName(item.familyId, families) }}
    </template>

    <!-- Date of Birth column -->
    <template #item.dateOfBirth="{ item }">
      {{ formatDate(item.dateOfBirth) }}
    </template>

    <!-- Gender column -->
    <template #item.gender="{ item }">
      <v-chip label size="small" class="text-capitalize">
        {{ item.gender }}
      </v-chip>
    </template>

    <!-- Actions column -->
    <template #item.actions="{ item }">
      <v-btn icon size="small" variant="text" @click="editMember(item)">
        <v-icon>mdi-pencil</v-icon>
      </v-btn>
      <v-btn icon size="small" variant="text" @click="confirmDelete(item)">
        <v-icon>mdi-delete</v-icon>
      </v-btn>
    </template>

    <!-- Loading state -->
    <template #loading>
      <v-skeleton-loader type="table-row@5" />
    </template>
  </v-data-table-server>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue';
import { useI18n } from 'vue-i18n';
import type { Member } from '@/types/member';
import type { Family } from '@/types/family';
import type { DataTableHeader } from 'vuetify';

const props = defineProps({
  members: {
    type: Array as () => Member[],
    required: true,
  },
  totalMembers: {
    type: Number,
    required: true,
  },
  loading: {
    type: Boolean,
    required: true,
  },
  families: {
    type: Array as () => Family[],
    required: true,
  },
});

console.log('MemberList received members:', props.members);
console.log('MemberList received totalMembers:', props.totalMembers);
console.log('MemberList received loading:', props.loading);
console.log('MemberList received families:', props.families);

const emit = defineEmits(['update:options', 'view', 'edit', 'delete', 'create']);

const { t } = useI18n();

const itemsPerPage = ref(10);

const headers = computed<DataTableHeader[]>(() => [
  { title: t('member.list.headers.avatar'), key: 'avatarUrl', sortable: false, width: '80px', align: 'center' },
  { title: t('member.list.headers.fullName'), key: 'fullName', width: 'auto', align: 'start' },
  { title: t('member.list.headers.family'), key: 'family', width: 'auto', align: 'start' },
  { title: t('member.list.headers.dateOfBirth'), key: 'dateOfBirth', width: '120px', align: 'center' },
  { title: t('member.list.headers.gender'), key: 'gender', width: '100px', align: 'center' },
  { title: t('member.list.headers.actions'), key: 'actions', sortable: false, width: '120px', align: 'center' },
]);

import { formatDate } from '@/utils/dateUtils';

const getFamilyName = (familyId: string, families: Family[]) => {
  const family = families.find(f => f.id === familyId);
  return family ? family.name : 'N/A';
};

const loadMembers = (options: { page: number; itemsPerPage: number; sortBy: string | string[] | null }) => {
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
