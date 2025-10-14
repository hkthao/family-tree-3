import type { Family, Member } from '@/types';

export interface GenerateDataRequest {
  prompt: string;
}

export interface GeneratedDataResponse {
  dataType: 'Family' | 'Member' | 'Unknown';
  family?: Family;
  member?: Member;
}
