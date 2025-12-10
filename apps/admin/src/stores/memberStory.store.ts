import { DEFAULT_ITEMS_PER_PAGE } from '@/constants/pagination';
import i18n from '@/plugins/i18n';
import type { DetectedFace, SearchResult, Member, Result, SearchStoriesFilter } from '@/types';
import type { MemberStoryDto } from '@/types/memberStory';
import type { GenerateStoryCommand, GenerateStoryResponseDto } from '@/types/ai';
import { defineStore } from 'pinia';
import type { ApiError } from '@/plugins/axios';


export interface MemberStoryFaceState {
  uploadedImage: string | null;
  resizedImageUrl: string | null;
  detectedFaces: DetectedFace[];
  selectedFaceId: string | undefined;
  faceSearchResults: SearchResult[];
  loading: boolean;
  error: string | null;
  originalImageUrl: string | null;
}

export const useMemberStoryStore = defineStore('memberStory', {
  state: () => ({

    error: null as string | null,


    list: {
      items: [] as MemberStoryDto[],
      loading: false,
      filters: {
        memberId: undefined,
        searchQuery: '',
      } as SearchStoriesFilter,
      currentPage: 1,
      itemsPerPage: DEFAULT_ITEMS_PER_PAGE,
      totalItems: 0,
      totalPages: 1,
      sortBy: [] as { key: string; order: string }[],
    },


    detail: {
      item: null as MemberStoryDto | null,
      loading: false,
    },


    add: {
      loading: false,
    },


    update: {
      loading: false,
    },


    _delete: {
      loading: false,
    },

    faceRecognition: {
      uploadedImage: null,
      uploadedImageId: null,
      detectedFaces: [],
      selectedFaceId: undefined,
      faceSearchResults: [],
      loading: false,
      error: null,
      resizedImageUrl: null,
      originalImageUrl: null
    } as MemberStoryFaceState,


    aiAnalysis: {
      loading: false,
      error: null as string | null,
      result: null,
    },
  }),



  actions: {
    async _loadItems() {
      this.list.loading = true;
      this.error = null;
      const result = await this.services.memberStory.search(
        {
          page: this.list.currentPage,
          itemsPerPage: this.list.itemsPerPage,
          sortBy: this.list.sortBy.map(s => ({ key: s.key, order: s.order as 'asc' | 'desc' })),
        },
        {
          memberId: this.list.filters.memberId,
          searchQuery: this.list.filters.searchQuery,
        }
      );

      if (result.ok) {
        this.list.items = result.value.items;
        this.list.totalItems = result.value.totalItems;
        this.list.totalPages = result.value.totalPages;
      } else {
        this.error = i18n.global.t('memberStory.errors.load');
        this.list.items = [];
        this.list.totalItems = 0;
        this.list.totalPages = 1;
        console.error(result.error);
      }
      this.list.loading = false;
    },

    async addItem(newItem: MemberStoryDto): Promise<Result<MemberStoryDto, ApiError>> {
      this.add.loading = true;
      this.error = null;
      const result = await this.services.memberStory.add(newItem);
      if (result.ok) {
        await this._loadItems();
      } else {
        this.error = i18n.global.t('memberStory.errors.add');
        console.error(result.error);
      }
      this.add.loading = false;
      return result;
    },

    async updateItem(updatedItem: MemberStoryDto): Promise<Result<MemberStoryDto, ApiError>> {
      this.update.loading = true;
      this.error = null;
      const result = await this.services.memberStory.update(updatedItem);
      if (result.ok) {
        await this._loadItems();
      } else {
        this.error = i18n.global.t('memberStory.errors.update');
        console.error(result.error);
      }
      this.update.loading = false;
      return result;
    },

    async deleteItem(id: string): Promise<Result<void, ApiError>> {
      this._delete.loading = true;
      this.error = null;
      const result = await this.services.memberStory.delete(id);
      if (result.ok) {
        await this._loadItems();
      } else {
        this.error = result.error.message || i18n.global.t('memberStory.errors.delete');
        console.error(result.error);
      }
      this._delete.loading = false;
      return result;
    },

    setListOptions(options: { page: number; itemsPerPage: number; sortBy: { key: string; order: string }[] }) {
      if (this.list.currentPage !== options.page) {
        this.list.currentPage = options.page;
      }

      if (this.list.itemsPerPage !== options.itemsPerPage) {
        this.list.itemsPerPage = options.itemsPerPage;
      }

      const currentSortBy = JSON.stringify(this.list.sortBy);
      const newSortBy = JSON.stringify(options.sortBy);
      if (currentSortBy !== newSortBy) {
        this.list.sortBy = options.sortBy;
      }

      this._loadItems();
    },

    async getById(id: string): Promise<MemberStoryDto | undefined> {
      this.detail.loading = true;
      this.error = null;
      const result = await this.services.memberStory.getById(id);
      this.detail.loading = false;
      if (result.ok) {
        if (result.value) {
          this.detail.item = result.value;
          return result.value;
        }
      } else {
        this.error = i18n.global.t('memberStory.errors.loadById');
        console.error(result.error);
      }
      return undefined;
    },

    setFilters(filters: SearchStoriesFilter) {
      this.list.filters = { ...this.list.filters, ...filters };
    },



    async generateStory(command: GenerateStoryCommand): Promise<Result<GenerateStoryResponseDto, ApiError>> {
      this.aiAnalysis.loading = true;
      this.error = null;
      try {
        const result = await this.services.ai.generateStory(command);
        if (!result.ok) {
          this.error = result.error?.message || i18n.global.t('memberStory.errors.storyGenerationFailed');
        }
        return result;
      } catch (error: any) {
        this.error = error.message || i18n.global.t('memberStory.errors.unexpectedError');
        return { ok: false, error: { message: this.error } as ApiError };
      } finally {
        this.aiAnalysis.loading = false;
      }
    },


    async detectFaces(imageFile: File, familyId: string, resizeImageForAnalysis: boolean): Promise<Result<void, ApiError>> {
      this.faceRecognition.loading = true;
      this.faceRecognition.error = null;
      try {
        // TODO: Ensure familyId is correctly passed from the context where detectFaces is called.
        const result = await this.services.memberFace.detect(imageFile, familyId, resizeImageForAnalysis);
        if (result.ok) {
          this.faceRecognition.uploadedImage = URL.createObjectURL(imageFile);
          this.faceRecognition.detectedFaces = result.value.detectedFaces.map((face) => ({
            id: face.id,
            boundingBox: face.boundingBox,
            thumbnail: face.thumbnail,
            memberId: face.memberId,
            originalMemberId: face.memberId,
            memberName: face.memberName,
            familyId: face.familyId,
            familyName: face.familyName,
            birthYear: face.birthYear,
            deathYear: face.deathYear,
            embedding: face.embedding,
            emotion: face.emotion,
            emotionConfidence: face.emotionConfidence,
            status: face.status || (face.memberId ? 'recognized' : 'unrecognized'),
          }));

          this.faceRecognition.resizedImageUrl = result.value.resizedImageUrl ?? null;
          this.faceRecognition.originalImageUrl = result.value.originalImageUrl ?? null;
          return { ok: true, value: undefined };
        } else {
          this.faceRecognition.error =
            result.error?.message ||
            i18n.global.t('face.errors.detectionFailed');
          return { ok: false, error: result.error };
        }
      } catch (err: any) {
        this.faceRecognition.error =
          err.message || i18n.global.t('face.errors.unexpectedError');
        return { ok: false, error: { message: this.faceRecognition.error } as ApiError };
      } finally {
        this.faceRecognition.loading = false;
      }
    },

    selectFace(faceId: string | undefined): void {
      this.faceRecognition.selectedFaceId = faceId;
    },

    async labelFace(
      faceId: string,
      memberId: string,
      memberDetails: Member,
    ): Promise<void> {
      const faceIndex = this.faceRecognition.detectedFaces.findIndex((f) => f.id === faceId);
      if (faceIndex !== -1) {
        this.faceRecognition.detectedFaces[faceIndex].memberId = memberId;
        this.faceRecognition.detectedFaces[faceIndex].status = 'labeled';
        if (memberDetails) {
          this.faceRecognition.detectedFaces[faceIndex].memberName = memberDetails.fullName;
          this.faceRecognition.detectedFaces[faceIndex].familyId = memberDetails.familyId;
          this.faceRecognition.detectedFaces[faceIndex].familyName = memberDetails.familyName;
          this.faceRecognition.detectedFaces[faceIndex].birthYear = memberDetails.dateOfBirth
            ? new Date(memberDetails.dateOfBirth).getFullYear()
            : undefined;
          this.faceRecognition.detectedFaces[faceIndex].deathYear = memberDetails.dateOfDeath
            ? new Date(memberDetails.dateOfDeath).getFullYear()
            : undefined;
        }
      }
    },

    removeFace(faceId: string): void {
      this.faceRecognition.detectedFaces = this.faceRecognition.detectedFaces.filter(
        (face) => face.id !== faceId,
      );
    },

    async fetchMemberDetails(memberId: string): Promise<Member | undefined> {
      const result = await this.services.member.getById(memberId);
      if (result.ok && result.value) {
        return result.value;
      } else if (!result.ok) {
        const errorResult = result as { ok: false; error: ApiError };
        console.error("Failed to fetch member details for ID:", memberId, errorResult.error);
        return undefined;
      }
    },

    resetFaceRecognitionState(): void {
      this.faceRecognition.uploadedImage = null;
      this.faceRecognition.detectedFaces = [];
      this.faceRecognition.selectedFaceId = undefined;
      this.faceRecognition.faceSearchResults = [];
      this.faceRecognition.loading = false;
      this.faceRecognition.error = null;
    },
  },
});