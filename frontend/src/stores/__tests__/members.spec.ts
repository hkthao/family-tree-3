import { setActivePinia, createPinia } from 'pinia';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { useMembersStore } from '../members';
import { MockMemberService, type Member } from '../../services/member.service';

describe('useMembersStore', () => {
  let mockService: MockMemberService;
  let initialMembers: Member[];

  beforeEach(() => {
    setActivePinia(createPinia());
    vi.resetAllMocks();

    initialMembers = [
      { id: '1', familyId: 'f1', fullName: 'Member A', gender: 'Male', status: 'Alive', generation: 1, createdAt: new Date().toISOString(), updatedAt: new Date().toISOString() },
      { id: '2', familyId: 'f1', fullName: 'Member B', gender: 'Female', status: 'Alive', generation: 1, createdAt: new Date().toISOString(), updatedAt: new Date().toISOString() },
      { id: '3', familyId: 'f2', fullName: 'Member C', gender: 'Other', status: 'Deceased', generation: 2, createdAt: new Date().toISOString(), updatedAt: new Date().toISOString() },
    ];
    mockService = new MockMemberService(initialMembers);
  });

  it('should fetch all members', async () => {
    const store = useMembersStore(mockService);
    await store.fetchAll();

    expect(store.members.length).toBe(initialMembers.length);
    expect(store.total).toBe(initialMembers.length);
    expect(store.loading).toBe(false);
    expect(mockService.fetchMembers).toHaveBeenCalledTimes(1);
  });

  it('should add a member', async () => {
    const store = useMembersStore(mockService);
    const newMemberData = {
      familyId: 'f3',
      fullName: 'New Member',
      gender: 'Male',
      status: 'Alive',
      generation: 1,
    };

    await store.add(newMemberData);

    expect(store.members.length).toBe(initialMembers.length + 1);
    expect(store.total).toBe(initialMembers.length + 1);
    expect(store.members.some(m => m.fullName === newMemberData.fullName)).toBe(true);
    expect(store.loading).toBe(false);
    expect(mockService.addMember).toHaveBeenCalledTimes(1);
    expect(mockService.addMember).toHaveBeenCalledWith(newMemberData);
  });

  it('should update a member', async () => {
    const store = useMembersStore(mockService);
    const memberToUpdate = initialMembers[0];
    const updatedName = 'Updated Member Name';

    await store.update(memberToUpdate.id, { fullName: updatedName });

    expect(store.members.find(m => m.id === memberToUpdate.id)?.fullName).toBe(updatedName);
    expect(store.loading).toBe(false);
    expect(mockService.updateMember).toHaveBeenCalledTimes(1);
    expect(mockService.updateMember).toHaveBeenCalledWith(memberToUpdate.id, { fullName: updatedName });
  });

  it('should delete a member', async () => {
    const store = useMembersStore(mockService);
    const memberToDelete = initialMembers[0];

    await store.remove(memberToDelete.id);

    expect(store.members.length).toBe(initialMembers.length - 1);
    expect(store.total).toBe(initialMembers.length - 1);
    expect(store.members.some(m => m.id === memberToDelete.id)).toBe(false);
    expect(store.loading).toBe(false);
    expect(mockService.removeMember).toHaveBeenCalledTimes(1);
    expect(mockService.removeMember).toHaveBeenCalledWith(memberToDelete.id);
  });

  it('should fetch a single member by id', async () => {
    const store = useMembersStore(mockService);
    const memberId = initialMembers[0].id;
    const member = await store.fetchOne(memberId);

    expect(member).toEqual(initialMembers[0]);
    expect(store.loading).toBe(false);
    expect(mockService.fetchMemberById).toHaveBeenCalledTimes(1);
    expect(mockService.fetchMemberById).toHaveBeenCalledWith(memberId);
  });

  it('should handle fetchAll error', async () => {
    const store = useMembersStore(mockService);
    vi.spyOn(mockService, 'fetchMembers').mockRejectedValueOnce(new Error('Fetch error'));
    vi.spyOn(console, 'error').mockImplementation(() => {});

    await store.fetchAll();

    expect(store.error).toBe('Fetch error');
    expect(store.loading).toBe(false);
  });

  it('should handle add error', async () => {
    const store = useMembersStore(mockService);
    vi.spyOn(mockService, 'addMember').mockRejectedValueOnce(new Error('Add error'));
    vi.spyOn(console, 'error').mockImplementation(() => {});

    const newMemberData = {
      familyId: 'f3',
      fullName: 'New Member',
      gender: 'Male',
      status: 'Alive',
      generation: 1,
    };

    await expect(store.add(newMemberData)).rejects.toThrow('Add error');
    expect(store.error).toBe('Add error');
    expect(store.loading).toBe(false);
  });

  it('should handle update error', async () => {
    const store = useMembersStore(mockService);
    vi.spyOn(mockService, 'updateMember').mockRejectedValueOnce(new Error('Update error'));
    vi.spyOn(console, 'error').mockImplementation(() => {});

    const memberToUpdate = initialMembers[0];
    const updatedName = 'Updated Member Name';

    await expect(store.update(memberToUpdate.id, { fullName: updatedName })).rejects.toThrow('Update error');
    expect(store.error).toBe('Update error');
    expect(store.loading).toBe(false);
  });

  it('should handle remove error', async () => {
    const store = useMembersStore(mockService);
    vi.spyOn(mockService, 'removeMember').mockRejectedValueOnce(new Error('Remove error'));
    vi.spyOn(console, 'error').mockImplementation(() => {});

    const memberToDelete = initialMembers[0];

    await expect(store.remove(memberToDelete.id)).rejects.toThrow('Remove error');
    expect(store.error).toBe('Remove error');
    expect(store.loading).toBe(false);
  });

  it('should handle fetchOne error', async () => {
    const store = useMembersStore(mockService);
    vi.spyOn(mockService, 'fetchMemberById').mockRejectedValueOnce(new Error('Fetch One error'));
    vi.spyOn(console, 'error').mockImplementation(() => {});

    const memberId = initialMembers[0].id;

    const result = await store.fetchOne(memberId);

    expect(result).toBeUndefined();
    expect(store.error).toBe('Fetch One error');
    expect(store.loading).toBe(false);
  });
});
