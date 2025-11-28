import { DEFAULT_ITEMS_PER_PAGE } from '@/constants/pagination';
import i18n from '@/plugins/i18n';
import type { DetectedFace, SearchResult, Member, Result, SearchStoriesFilter } from '@/types'; // Removed Paginated
import type { MemberStoryDto } from '@/types/memberStory'; // Updated
import type { AiPhotoAnalysisInputDto, PhotoAnalysisResultDto, GenerateStoryCommand, GenerateStoryResponseDto } from '@/types/ai'; // NEW IMPORT
import { defineStore } from 'pinia';
import type { ApiError } from '@/plugins/axios';

export interface MemberStoryFaceState { // Updated
  uploadedImage: string | null; // Base64 or URL of the uploaded image
  uploadedImageId: string | null; // ID of the uploaded image from the backend
  resizedImageUrl: string | null; // NEW: URL of the resized image for analysis
  detectedFaces: DetectedFace[]; // Array of detected faces with bounding boxes
  selectedFaceId: string | undefined; // ID of the currently selected face for labeling
  faceSearchResults: SearchResult[]; // Results from face search
  loading: boolean;
  error: string | null;
}

export const useMemberStoryStore = defineStore('memberStory', { // Updated
  state: () => ({
    // General state
    error: null as string | null,

    // State for list operations
    list: {
      items: [] as MemberStoryDto[], // Updated
      loading: false, // Loading state for the list of member stories // Updated
      filters: {
        memberId: undefined, // Default to undefined for filtering
        searchQuery: '',
      } as SearchStoriesFilter, // Updated
      currentPage: 1,
      itemsPerPage: DEFAULT_ITEMS_PER_PAGE,
      totalItems: 0,
      totalPages: 1,
      sortBy: [] as { key: string; order: string }[], // Sorting key and order
    },

    // State for single item operations
    detail: {
      item: null as MemberStoryDto | null, // Updated
      loading: false, // Loading state for a single member story // Updated
    },

    // State for add operations
    add: {
      loading: false,
    },

    // State for update operations
    update: {
      loading: false,
    },

    // State for delete operations
    _delete: {
      loading: false,
    },

    // Face Recognition State for member story creation // Updated
    faceRecognition: {
      uploadedImage: null,
      uploadedImageId: null,
      detectedFaces: [],
      selectedFaceId: undefined,
      faceSearchResults: [],
      loading: false,
      error: null,
      resizedImageUrl: null
    } as MemberStoryFaceState, // Updated

    // AI Analysis State for photo analysis
    aiAnalysis: { // NEW STATE
      loading: false,
      error: null as string | null,
      result: null as PhotoAnalysisResultDto | null,
    },
  }),

  getters: {
    headers: (_state) => { // Renamed state to _state
      const t = i18n.global.t;
      return [
        {
          title: t('memberStory.list.header.title'), // Updated
          align: 'start' as const,
        },
        {
          title: t('member.list.headers.fullName'), // Assuming we want to show member name
          key: 'memberId',
          align: 'start' as const,
        },
        {
          title: t('memberStory.list.header.actions'), // Updated
          key: 'actions',
          sortable: false,
          align: 'end' as const,
        },
      ];
    },
    // Getters for Face Recognition
    currentSelectedFace: (state) =>
      state.faceRecognition.detectedFaces.find((face) => face.id === state.faceRecognition.selectedFaceId),
    unlabeledFaces: (state) =>
      state.faceRecognition.detectedFaces.filter((face) => !face.memberId),
    labeledFaces: (state) =>
      state.faceRecognition.detectedFaces.filter((face) => face.memberId),
  },

  actions: {
    async _loadItems() {
      this.list.loading = true;
      this.error = null;
      const result = await this.services.memberStory.loadItems( // Updated
        {
          memberId: this.list.filters.memberId,
          searchQuery: this.list.filters.searchQuery,
          sortBy: this.list.sortBy.length > 0 ? this.list.sortBy[0].key : undefined,
          sortOrder: this.list.sortBy.length > 0 ? (this.list.sortBy[0].order as 'asc' | 'desc') : undefined,
        },
        this.list.currentPage,
        this.list.itemsPerPage,
      );

      if (result.ok) {
        this.list.items = result.value.items;
        this.list.totalItems = result.value.totalItems;
        this.list.totalPages = result.value.totalPages;
      } else {
        this.error = i18n.global.t('memberStory.errors.load'); // Updated
        this.list.items = [];
        this.list.totalItems = 0;
        this.list.totalPages = 1;
        console.error(result.error);
      }
      this.list.loading = false;
    },

    async addItem(newItem: MemberStoryDto): Promise<Result<MemberStoryDto, ApiError>> { // Updated
      this.add.loading = true;
      this.error = null;
      const result = await this.services.memberStory.add(newItem); // Updated
      if (result.ok) {
        await this._loadItems();
      } else {
        this.error = i18n.global.t('memberStory.errors.add'); // Updated
        console.error(result.error);
      }
      this.add.loading = false;
      return result;
    },

    async updateItem(updatedItem: MemberStoryDto): Promise<Result<MemberStoryDto, ApiError>> { // Updated
      this.update.loading = true;
      this.error = null;
      const result = await this.services.memberStory.update(updatedItem); // Updated
      if (result.ok) {
        await this._loadItems();
      } else {
        this.error = i18n.global.t('memberStory.errors.update'); // Updated
        console.error(result.error);
      }
      this.update.loading = false;
      return result;
    },

    async deleteItem(id: string): Promise<Result<void, ApiError>> {
      this._delete.loading = true;
      this.error = null;
      const result = await this.services.memberStory.delete(id); // Updated
      if (result.ok) {
        await this._loadItems();
      } else {
        this.error = result.error.message || i18n.global.t('memberStory.errors.delete'); // Updated
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

    async getById(id: string): Promise<MemberStoryDto | undefined> { // Updated
      this.detail.loading = true;
      this.error = null;
      const result = await this.services.memberStory.getById(id); // Updated
      this.detail.loading = false;
      if (result.ok) {
        if (result.value) {
          this.detail.item = result.value;
          return result.value;
        }
      } else {
        this.error = i18n.global.t('memberStory.errors.loadById'); // Updated
        console.error(result.error);
      }
      return undefined;
    },

    setFilters(filters: SearchStoriesFilter) { // Updated
      this.list.filters = { ...this.list.filters, ...filters };
    },

    async analyzePhoto(command: { Input: AiPhotoAnalysisInputDto }): Promise<Result<PhotoAnalysisResultDto, ApiError>> { // MODIFIED
      this.aiAnalysis.loading = true;
      this.aiAnalysis.error = null;
      try {
        const result = await this.services.ai.analyzePhoto(command); // Use new ai service
        if (result.ok) {
          this.aiAnalysis.result = result.value;
        } else {
          this.aiAnalysis.error = result.error?.message || i18n.global.t('memberStory.errors.aiAnalysisFailed'); // Updated
        }
        return result;
      } catch (error: any) {
        this.aiAnalysis.error = error.message || i18n.global.t('memberStory.errors.unexpectedError'); // Updated
        return { ok: false, error: { message: this.aiAnalysis.error } as ApiError };
      } finally {
        this.aiAnalysis.loading = false;
      }
    },

    async generateStory(command: GenerateStoryCommand): Promise<Result<GenerateStoryResponseDto, ApiError>> {
      this.aiAnalysis.loading = true; // Use aiAnalysis loading for AI operations
      this.error = null;
      try {
        const result = await this.services.ai.generateStory(command);
        if (!result.ok) {
          this.error = result.error?.message || i18n.global.t('memberStory.errors.storyGenerationFailed'); // Updated
        }
        return result;
      } catch (error: any) {
        this.error = error.message || i18n.global.t('memberStory.errors.unexpectedError'); // Updated
        return { ok: false, error: { message: this.error } as ApiError };
      } finally {
        this.aiAnalysis.loading = false;
      }
    },

    // Actions for Face Recognition
    async detectFaces(imageFile: File, resizeImageForAnalysis: boolean): Promise<Result<void, ApiError>> {
      this.faceRecognition.loading = true;
      this.faceRecognition.error = null;
      try {
        const result = await this.services.face.detect(imageFile, resizeImageForAnalysis);
        if (result.ok) {
          this.faceRecognition.uploadedImage = URL.createObjectURL(imageFile);
          this.faceRecognition.uploadedImageId = result.value.imageId;
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
            status: face.memberId ? 'original-recognized' : 'unrecognized',
          }));
          // Store the resized image URL if available
          this.faceRecognition.resizedImageUrl = result.value.resizedImageUrl ?? null;
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

    async saveFaceLabels(): Promise<Result<void, ApiError>> {
      this.faceRecognition.loading = true;
      this.faceRecognition.error = null;
      try {
        const facesToSave = this.faceRecognition.detectedFaces.filter(
          (face) =>
            face.memberId &&
            (face.originalMemberId === null ||
              face.originalMemberId === undefined ||
              face.memberId !== face.originalMemberId),
        );

        if (facesToSave.length === 0) {
          this.faceRecognition.loading = false;
          return { ok: true, value: undefined };
        }

        const faceLabels = facesToSave.map((face) => ({
          id: face.id,
          boundingBox: face.boundingBox,
          thumbnail: face.thumbnail,
          memberId: face.memberId,
          memberName: face.memberName,
          familyId: face.familyId,
          familyName: face.familyName,
          birthYear: face.birthYear,
          deathYear: face.deathYear,
          embedding: face.embedding,
          emotion: face.emotion,
          emotionConfidence: face.emotionConfidence,
          status: face.status,
        }));

        const imageId = this.faceRecognition.uploadedImageId;

        if (!imageId) {
          const errorMessage = 'Image ID is missing. Cannot save face labels.';
          this.faceRecognition.error = errorMessage;
          return { ok: false, error: { message: errorMessage } as ApiError };
        }

        const result = await this.services.face.saveLabels(faceLabels, imageId);

        if (result.ok) {
          facesToSave.forEach((face) => {
            face.originalMemberId = face.memberId;
            face.status = 'original-recognized';
          });
          return { ok: true, value: undefined };
        } else {
          this.faceRecognition.error =
            result.error?.message ||
            i18n.global.t('face.errors.saveMappingFailed');
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
      this.faceRecognition.uploadedImageId = null;
      this.faceRecognition.detectedFaces = [];
      this.faceRecognition.selectedFaceId = undefined;
      this.faceRecognition.faceSearchResults = [];
      this.faceRecognition.loading = false;
      this.faceRecognition.error = null;
    },
  },
});