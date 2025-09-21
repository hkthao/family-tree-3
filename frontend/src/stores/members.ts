import { defineStore } from 'pinia';
import { ref } from 'vue';
import type { Member, MemberServiceType } from '../services/member.service';

export const useMembersStore = defineStore('members', (memberService: MemberServiceType) => {
  const members = ref<Member[]>([]);
  const loading = ref(false);
  const error = ref<string | null>(null);
  const total = ref(0);

  async function fetchAll(search = '', familyId?: string, page = 1, perPage = 10) {
    loading.value = true;
    error.value = null;
    try {
      const response = await memberService.fetchMembers(search, familyId, page, perPage);
      members.value = response.items;
      total.value = response.total;
    } catch (err: unknown) {
      error.value = (err as Error).message || 'Failed to fetch members';
      console.error(err);
    } finally {
      loading.value = false;
    }
  }

  async function fetchOne(id: string): Promise<Member | undefined> {
    loading.value = true;
    error.value = null;
    try {
      const member = await memberService.fetchMemberById(id);
      return member;
    } catch (err: unknown) {
      error.value = (err as Error).message || 'Failed to fetch member';
      console.error(err);
      return undefined;
    } finally {
      loading.value = false;
    }
  }

  async function add(member: Omit<Member, 'id' | 'createdAt' | 'updatedAt'>) {
    loading.value = true;
    error.value = null;
    try {
      const newMember = await memberService.addMember(member);
      members.value.push(newMember);
      total.value++;
    } catch (err: unknown) {
      error.value = (err as Error).message || 'Failed to add member';
      console.error(err);
      throw err as Error;
    } finally {
      loading.value = false;
    }
  }

  async function update(id: string, member: Partial<Omit<Member, 'id' | 'createdAt' | 'updatedAt'>>) {
    loading.value = true;
    error.value = null;
    try {
      await memberService.updateMember(id, member);
      const index = members.value.findIndex(m => m.id === id);
      if (index !== -1) {
        members.value[index] = { ...members.value[index], ...member, updatedAt: new Date().toISOString() };
      }
    } catch (err: unknown) {
      error.value = (err as Error).message || 'Failed to update member';
      console.error(err);
      throw err as Error;
    } finally {
      loading.value = false;
    }
  }

  async function remove(id: string) {
    loading.value = true;
    error.value = null;
    try {
      await memberService.removeMember(id);
      members.value = members.value.filter(m => m.id !== id);
      total.value--;
    } catch (err: unknown) {
      error.value = (err as Error).message || 'Failed to delete member';
      console.error(err);
      throw err as Error;
    } finally {
      loading.value = false;
    }
  }

  return {
    members,
    loading,
    error,
    total,
    fetchAll,
    fetchOne,
    add,
    update,
    remove,
  };
});
