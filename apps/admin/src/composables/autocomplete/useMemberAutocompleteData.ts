import { ref, watch } from 'vue';
import type { Member } from '@/types';
import { ApiMemberService } from '@/services/member/api.member.service';
import apiClient from '@/plugins/axios';

const memberService = new ApiMemberService(apiClient);

export function useMemberAutocompleteData(
  modelValue: string | string[] | undefined | null,
  multiple: boolean,
  familyId?: string, // Prop for filtering by family
) {
  const preloadedMembers = ref<Member[]>([]);
  const internalValue = ref<Member | Member[] | null>(null);
  const isLoadingPreload = ref(false);

  const fetchMemberByIds = async (ids: string[]) => {
    if (!ids || ids.length === 0) {
      preloadedMembers.value = [];
      return;
    }
    isLoadingPreload.value = true;
    try {
      const result = await memberService.getByIds(ids);
      if (result.ok) {
        preloadedMembers.value = result.value;
      } else {
        console.error('Error preloading members:', result.error);
        preloadedMembers.value = [];
      }
    } finally {
      isLoadingPreload.value = false;
    }
  };

  watch(() => modelValue, async (newModelValue) => {
    if (newModelValue) {
      if (multiple && Array.isArray(newModelValue)) {
        await fetchMemberByIds(newModelValue as string[]);
        internalValue.value = preloadedMembers.value;
      } else if (!multiple && typeof newModelValue === 'string') {
        await fetchMemberByIds([newModelValue as string]);
        internalValue.value = preloadedMembers.value[0] || null;
      }
    } else {
      internalValue.value = null;
    }
  }, { immediate: true });

  const fetchItems = async (query: string): Promise<Member[]> => {
    const filters: { [key: string]: any } = {};
    if (query) {
      filters.fullName = query;
    }
    if (familyId) { // Apply familyId filter if provided
      filters.familyId = familyId;
    }

    // Only search if there's a query or a familyId is provided
    if (!query && !familyId) {
      return [];
    }

    const result = await memberService.search({ page: 1, itemsPerPage: 10 }, filters);
    if (result.ok) {
      return result.value.items;
    }
    console.error('Error fetching members:', result.error);
    return [];
  };

  return {
    internalValue,
    isLoadingPreload,
    fetchItems,
  };
}
