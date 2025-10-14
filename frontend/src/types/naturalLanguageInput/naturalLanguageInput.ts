import type { Family, Member } from '@/types';

export interface GenerateDataRequest {
  prompt: string;
}

export interface GeneratedDataResponse {
  dataType: 'Families' | 'Members' | 'Mixed' | 'Unknown';
  families: Family[];
  members: Member[];
}
