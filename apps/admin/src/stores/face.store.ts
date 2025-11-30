import { defineStore } from 'pinia';
import type { DetectedFace, SearchResult, Member, Result } from '@/types';
import i18n from '@/plugins/i18n';
import type { ApiError } from '@/plugins/axios';

interface FaceState {
  uploadedImage: string | null;
  uploadedImageId: string | null;
  resizedImageUrl: string | null;
  originalImageUrl: string | null;
  detectedFaces: DetectedFace[];
  selectedFaceId: string | undefined;
  faceSearchResults: SearchResult[];
  loading: boolean;
  error: string | null;
}

export const useFaceStore = defineStore('face', {
  state: (): FaceState => ({
    uploadedImage: null,
    uploadedImageId: null,
    detectedFaces: [],
    selectedFaceId: undefined,
    faceSearchResults: [],
    loading: false,
    error: null,
    resizedImageUrl: null,
    originalImageUrl: null,
  }),

  getters: {

    currentSelectedFace: (state) =>
      state.detectedFaces.find((face) => face.id === state.selectedFaceId),
    unlabeledFaces: (state) =>
      state.detectedFaces.filter((face) => !face.memberId),
    labeledFaces: (state) =>
      state.detectedFaces.filter((face) => face.memberId),
  },

  actions: {

    async detectFaces(imageFile: File, resizeImageForAnalysis: boolean): Promise<Result<void, ApiError>> {
      this.loading = true;
      this.error = null;
      try {
        const result = await this.services.memberFace.detect(imageFile, resizeImageForAnalysis);
        if (result.ok) {
          this.uploadedImage = URL.createObjectURL(imageFile);
          this.uploadedImageId = result.value.imageId;
          this.resizedImageUrl = result.value.resizedImageUrl ?? null;
          this.originalImageUrl = result.value.originalImageUrl ?? null;
          this.detectedFaces = result.value.detectedFaces.map((face) => ({
            id: face.id,
            boundingBox: face.boundingBox,
            thumbnail: face.thumbnail,
            thumbnailUrl: face.thumbnailUrl,
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
            status: face.status || (face.memberId ? 'original-recognized' : 'unrecognized'),
          }));
          return { ok: true, value: undefined };
        } else {
          this.error =
            result.error?.message ||
            i18n.global.t('face.errors.detectionFailed');
          return { ok: false, error: result.error };
        }
      } catch (err: any) {
        this.error =
          err.message || i18n.global.t('face.errors.unexpectedError');
        return { ok: false, error: { message: this.error } as ApiError };
      } finally {
        this.loading = false;
      }
    },


    selectFace(faceId: string | undefined): void {
      this.selectedFaceId = faceId;
    },


    async labelFace(
      faceId: string,
      memberId: string,
      memberDetails: Member,
    ): Promise<void> {
      const faceIndex = this.detectedFaces.findIndex((f) => f.id === faceId);
      if (faceIndex !== -1) {
        this.detectedFaces[faceIndex].memberId = memberId;
        this.detectedFaces[faceIndex].status = 'labeled';
        if (memberDetails) {
          this.detectedFaces[faceIndex].memberName = memberDetails.fullName;
          this.detectedFaces[faceIndex].familyId = memberDetails.familyId;
          this.detectedFaces[faceIndex].familyName = memberDetails.familyName;
          this.detectedFaces[faceIndex].birthYear = memberDetails.dateOfBirth
            ? new Date(memberDetails.dateOfBirth).getFullYear()
            : undefined;
          this.detectedFaces[faceIndex].deathYear = memberDetails.dateOfDeath
            ? new Date(memberDetails.dateOfDeath).getFullYear()
            : undefined;
        }
      }
    },


    removeFace(faceId: string): void {
      this.detectedFaces = this.detectedFaces.filter(
        (face) => face.id !== faceId,
      );
    },

    resetState(): void {
      this.uploadedImage = null;
      this.uploadedImageId = null;
      this.detectedFaces = [];
      this.selectedFaceId = undefined;
      this.faceSearchResults = [];
      this.loading = false;
      this.error = null;
    },
  },
});

