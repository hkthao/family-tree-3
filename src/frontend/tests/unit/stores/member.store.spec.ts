
import { useMemberStore } from '@/stores/member.store';
import { defineCrudTests } from '../../shared/crudTests';
import { MockMemberService } from '../../shared/mock.services';
import type { Member } from '@/types';
import { Gender } from '@/types';

const mockMemberService = new MockMemberService();

const entitySample: Member = {
  id: 'member-001',
  familyId: 'family-001',
  lastName: 'Test',
  firstName: 'Member',
  gender: Gender.Male,
  fullName: 'Test Member',
};

defineCrudTests(
  'Member Store',
  useMemberStore,
  mockMemberService,
  'member',
  entitySample,
);
