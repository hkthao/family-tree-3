import { ref, computed, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import type { FamilyDict } from '@/types';
import type { DataTableHeader } from 'vuetify';

import { DEFAULT_ITEMS_PER_PAGE } from '@/constants/pagination';
import { FamilyDictType, FamilyDictLineage } from '@/types';
import { useAuth } from '@/composables';

interface FamilyDictListProps {
  items: FamilyDict[];
  totalItems: number;
  loading: boolean;
  search?: string;
  readOnly?: boolean;
}

export function useFamilyDictList(props: FamilyDictListProps, emit: any) {
  const { t } = useI18n();
  const { state } = useAuth();

  const canPerformActions = computed(() => {
    return !props.readOnly && state.isAdmin.value;
  });

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

  const itemsPerPage = ref(DEFAULT_ITEMS_PER_PAGE);

  const getFamilyDictTypeTitle = (type: FamilyDictType) => {
    switch (type) {
      case FamilyDictType.Blood: return t('familyDict.type.blood');
      case FamilyDictType.Marriage: return t('familyDict.type.marriage');
      case FamilyDictType.Adoption: return t('familyDict.type.adoption');
      case FamilyDictType.InLaw: return t('familyDict.type.inLaw');
      case FamilyDictType.Other: return t('familyDict.type.other');
      default: return t('common.unknown');
    }
  };

  const getFamilyDictLineageTitle = (lineage: FamilyDictLineage) => {
    switch (lineage) {
      case FamilyDictLineage.Noi: return t('familyDict.lineage.noi');
      case FamilyDictLineage.Ngoai: return t('familyDict.lineage.ngoai');
      case FamilyDictLineage.NoiNgoai: return t('familyDict.lineage.noiNgoai');
      case FamilyDictLineage.Other: return t('familyDict.lineage.other');
      default: return t('common.unknown');
    }
  };

  const headers = computed<DataTableHeader[]>(() => {
    const baseHeaders: DataTableHeader[] = [
      {
        title: t('familyDict.list.headers.name'),
        key: 'name',
        width: 'auto',
        align: 'start',
      },
      {
        title: t('familyDict.list.headers.type'),
        key: 'type',
        width: '150px',
        align: 'center',
      },
      {
        title: t('familyDict.list.headers.lineage'),
        key: 'lineage',
        width: '150px',
        align: 'center',
      },
      {
        title: t('familyDict.list.headers.namesByRegion'),
        key: 'namesByRegion',
        width: '250px',
        align: 'start',
        sortable: false,
      },
    ];

    if (canPerformActions.value) {
      baseHeaders.push({
        title: t('familyDict.list.headers.actions'),
        key: 'actions',
        sortable: false,
        align: 'end',
      });
    }
    return baseHeaders;
  });

  const loadFamilyDicts = (options: {
    page: number;
    itemsPerPage: number;
    sortBy: { key: string; order: string }[];
  }) => {
    emit('update:options', options);
  };

  const viewFamilyDict = (familyDict: FamilyDict) => {
    emit('view', familyDict);
  };

  const editFamilyDict = (familyDict: FamilyDict) => {
    emit('edit', familyDict);
  };

  const confirmDelete = (familyDict: FamilyDict) => {
    emit('delete', familyDict);
  };

  return {
    debouncedSearch,
    itemsPerPage,
    headers,
    canPerformActions,
    getFamilyDictTypeTitle,
    getFamilyDictLineageTitle,
    loadFamilyDicts,
    viewFamilyDict,
    editFamilyDict,
    confirmDelete,
  };
}
