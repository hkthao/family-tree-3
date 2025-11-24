import { defineStore } from 'pinia';
import apiClient from '@/plugins/axios';
import type { ApiError } from '@/types'; // Import ApiError
import type {
  PhotoAnalysisResultDto,
  GenerateStoryRequestDto, GenerateStoryResponseDto,
  CreateMemoryDto, UpdateMemoryDto, MemoryDto
} from '@/types/memory.d';

export const useMemoryStore = defineStore('memory', {
  state: () => ({
    memories: {
      items: [] as MemoryDto[],
      totalCount: 0,
      page: 1,
      pageSize: 10,
    },
    memoryDetail: null as MemoryDto | null,
    loading: false,
    error: null as string | null,
  }),
  getters: {
    getMemories: (state) => state.memories.items,
    getMemoryDetail: (state) => state.memoryDetail,
    isLoading: (state) => state.loading,
    getError: (state) => state.error,
  },
  actions: {
    async analyzePhoto(formData: FormData) {
      this.loading = true;
      this.error = null;
      try {
        const result = await apiClient.post<PhotoAnalysisResultDto>('/memories/analyze-photo', formData, {
          headers: {
            'Content-Type': 'multipart/form-data',
          },
        });
        if (result.ok) { // Changed to result.ok
            return { isSuccess: true, value: result.value! };
        } else {
            this.error = result.error?.message || 'Unknown error during photo analysis'; // Access result.error
            return { isSuccess: false, error: this.error };
        }
      } catch (err: any) { // Catch unexpected errors outside of ApiError
        this.error = err.message || 'An unexpected error occurred';
        return { isSuccess: false, error: this.error };
      } finally {
        this.loading = false;
      }
    },

    async generateStory(payload: GenerateStoryRequestDto) {
      this.loading = true;
      this.error = null;
      try {
        const result = await apiClient.post<GenerateStoryResponseDto>('/memories/generate', payload);
        if (result.ok) { // Changed from isSuccess to ok
            return { isSuccess: true, value: result.value! };
        } else {
            this.error = result.error?.message || 'Unknown error during story generation';
            return { isSuccess: false, error: this.error };
        }
      } catch (err: any) {
        this.error = err.message || 'An unexpected error occurred';
        return { isSuccess: false, error: this.error };
      } finally {
        this.loading = false;
      }
    },

    async create(payload: CreateMemoryDto) {
      this.loading = true;
      this.error = null;
      try {
        const result = await apiClient.post<string>('/memories', payload); // Returns Guid as string
        if (result.ok) {
            return { isSuccess: true, value: result.value! };
        } else {
            this.error = result.error?.message || 'Unknown error during memory creation';
            return { isSuccess: false, error: this.error };
        }
      } catch (err: any) {
        this.error = err.message || 'An unexpected error occurred';
        return { isSuccess: false, error: this.error };
      } finally {
        this.loading = false;
      }
    },

    async update(payload: UpdateMemoryDto) {
      this.loading = true;
      this.error = null;
      try {
        const result = await apiClient.put(`/memories/${payload.id}`, payload);
        if (result.ok) {
            return { isSuccess: true };
        } else {
            this.error = result.error?.message || 'Unknown error during memory update';
            return { isSuccess: false, error: this.error };
        }
      } catch (err: any) {
        this.error = err.message || 'An unexpected error occurred';
        return { isSuccess: false, error: this.error };
      } finally {
        this.loading = false;
      }
    },

    async delete(memoryId: string) {
      this.loading = true;
      this.error = null;
      try {
        const result = await apiClient.delete(`/memories/${memoryId}`);
        if (result.ok) {
            return { isSuccess: true };
        } else {
            this.error = result.error?.message || 'Unknown error during memory deletion';
            return { isSuccess: false, error: this.error };
        }
      } catch (err: any) {
        this.error = err.message || 'An unexpected error occurred';
        return { isSuccess: false, error: this.error };
      } finally {
        this.loading = false;
      }
    },

    async getById(memoryId: string) {
      this.loading = true;
      this.error = null;
      try {
        const result = await apiClient.get<MemoryDto>(`/memories/detail/${memoryId}`);
        if (result.ok) {
            this.memoryDetail = result.value;
            return { isSuccess: true, value: result.value! };
        } else {
            this.error = result.error?.message || 'Unknown error fetching memory details';
            return { isSuccess: false, error: this.error };
        }
      } catch (err: any) {
        this.error = err.message || 'An unexpected error occurred';
        return { isSuccess: false, error: this.error };
      } finally {
        this.loading = false;
      }
    },

    async getByMemberId(memberId: string, params: { page?: number; limit?: number; search?: string; sortBy?: string } = {}) {
      this.loading = true;
      this.error = null;
      try {
        const queryParams = new URLSearchParams();
        if (params.page) queryParams.append('pageNumber', params.page.toString());
        if (params.limit) queryParams.append('pageSize', params.limit.toString());
        if (params.search) queryParams.append('search', params.search);
        if (params.sortBy) queryParams.append('sortBy', params.sortBy);

        const result = await apiClient.get<{ items: MemoryDto[]; totalCount: number; page: number; pageSize: number }>(`/memories/member/${memberId}?${queryParams.toString()}`);
        if (result.ok) {
            this.memories = {
                items: result.value?.items || [],
                totalCount: result.value?.totalCount || 0,
                page: result.value?.page || 1,
                pageSize: result.value?.pageSize || 10,
            };
            return { isSuccess: true, value: this.memories };
        } else {
            this.error = result.error?.message || 'Unknown error fetching memories by member ID';
            return { isSuccess: false, error: this.error };
        }
      } catch (err: any) {
        this.error = err.message || 'An unexpected error occurred';
        return { isSuccess: false, error: this.error };
      } finally {
        this.loading = false;
      }
    },
  },
});
