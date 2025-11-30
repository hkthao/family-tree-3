<template>
  <v-card :elevation="0">
    <v-card-title class="d-flex align-center">
      <div class="text-h6 text-uppercase">{{ t('memberFace.list.title') }}</div>
      <v-spacer></v-spacer>
      <v-btn
        color="primary"
        @click="emit('create')"
        data-testid="create-member-face-button"
      >
        <v-icon left>mdi-plus</v-icon>
        {{ t('common.create') }}
      </v-btn>
    </v-card-title>
    <v-card-text>
      <v-data-table-server
        v-model:items-per-page="itemsPerPage"
        v-model:page="page"
        v-model:sort-by="sortBy"
        :headers="headers"
        :items="items"
        :items-length="totalItems"
        :loading="loading"
        class="elevation-0"
        item-value="id"
        @update:options="handleUpdateOptions"
      >
        <template v-slot:item.thumbnail="{ item }">
          <v-img v-if="item.thumbnailUrl" :src="item.thumbnailUrl" height="40" width="40" cover class="my-1"></v-img>
          <v-icon v-else>mdi-image-off</v-icon>
        </template>
        <template v-slot:item.memberName="{ item }">
          <MemberName :fullName="item.memberName" :gender="item.memberGender" :avatarUrl="item.memberAvatarUrl" />
        </template>
        <template v-slot:item.familyName="{ item }">
          {{ item.familyName }}
        </template>
        <template v-slot:item.actions="{ item }">
          <v-menu>
            <template v-slot:activator="{ props }">
              <v-btn icon variant="text" size="small" v-bind="props" data-testid="member-face-actions-menu">
                <v-icon>mdi-dots-vertical</v-icon>
              </v-btn>
            </template>
            <v-list>
              <v-list-item @click="emit('view', item)" data-testid="view-member-face-button">
                <v-list-item-title>
                  <v-icon left>mdi-eye</v-icon>
                  {{ t('common.viewDetails') }}
                </v-list-item-title>
              </v-list-item>
              <v-list-item @click="emit('edit', item)" data-testid="edit-member-face-button">
                <v-list-item-title>
                  <v-icon left>mdi-pencil</v-icon>
                  {{ t('common.edit') }}
                </v-list-item-title>
              </v-list-item>
              <v-list-item @click="emit('delete', item)" color="error" data-testid="delete-member-face-button">
                <v-list-item-title>
                  <v-icon left>mdi-delete</v-icon>
                  {{ t('common.delete') }}
                </v-list-item-title>
              </v-list-item>
            </v-list>
          </v-menu>
        </template>
        <template #bottom></template>
      </v-data-table-server>
    </v-card-text>
    <v-card-actions class="d-flex justify-end">
      <v-pagination
        v-model="page"
        :length="Math.ceil(totalItems / itemsPerPage)"
        :total-visible="5"
        rounded="circle"
      ></v-pagination>
    </v-card-actions>
  </v-card>
</template>

<script setup lang="ts">
import { ref, watch, computed } from 'vue';
import { useI18n } from 'vue-i18n';
import type { MemberFace } from '@/types';
import MemberName from '@/components/member/MemberName.vue'; 

interface MemberFaceListProps {
  items: MemberFace[];
  totalItems: number;
  loading: boolean;
}

const props = defineProps<MemberFaceListProps>();
const emit = defineEmits(['update:options', 'view', 'edit', 'delete', 'create']);

const { t } = useI18n();

const page = ref(1);
const itemsPerPage = ref(10);
const sortBy = ref<any[]>([]); 

const headers = computed(() => [
  { title: t('memberFace.list.headers.thumbnail'), key: 'thumbnail', sortable: false, width: '80px' }, 
  { title: t('memberFace.list.headers.memberName'), key: 'memberName' },
  { title: t('memberFace.list.headers.familyName'), key: 'familyName' }, 
  { title: t('memberFace.list.headers.actions'), key: 'actions', sortable: false, width: '80px' },
]);


watch([page, itemsPerPage, sortBy], () => {
  emit('update:options', {
    page: page.value,
    itemsPerPage: itemsPerPage.value,
    sortBy: sortBy.value,
  });
}, { deep: true });


const handleUpdateOptions = (options: { page: number; itemsPerPage: number; sortBy: { key: string; order: string }[]; }) => {
  page.value = options.page;
  itemsPerPage.value = options.itemsPerPage;
  sortBy.value = options.sortBy;
};
</script>