import { defineStore } from 'pinia';
import type { DetectedFace, SearchResult, Member, Result } from '@/types';
import i18n from '@/plugins/i18n'; // For localization
import type { ApiError } from '@/plugins/axios';

interface FaceState {
  uploadedImage: string | null; // Base64 or URL of the uploaded image
  uploadedImageId: string | null; // ID of the uploaded image from the backend
  resizedImageUrl: string | null; // NEW: URL of the resized image for analysis
  detectedFaces: DetectedFace[]; // Array of detected faces with bounding boxes
  selectedFaceId: string | undefined; // ID of the currently selected face for labeling
  faceSearchResults: SearchResult[]; // Results from face search
  loading: boolean;
  error: string | null;
}

export const useFaceStore = defineStore('face', {
  state: (): FaceState => ({
    uploadedImage: null,
    uploadedImageId: null, // Initialize uploadedImageId
    detectedFaces: [],
    selectedFaceId: undefined,
    faceSearchResults: [],
    loading: false,
    error: null,
    resizedImageUrl: null
  }),

  getters: {
    // Getters to retrieve specific data from the state
    currentSelectedFace: (state) =>
      state.detectedFaces.find((face) => face.id === state.selectedFaceId),
    unlabeledFaces: (state) =>
      state.detectedFaces.filter((face) => !face.memberId),
    labeledFaces: (state) =>
      state.detectedFaces.filter((face) => face.memberId),
  },

  actions: {
    // Action to handle image upload and face detection
    async detectFaces(imageFile: File, resizeImageForAnalysis: boolean): Promise<Result<void, ApiError>> {
      this.loading = true;
      this.error = null;
      try {
        const result = await this.services.memberFace.detect(imageFile, resizeImageForAnalysis);
        if (result.ok) {
          this.uploadedImage = URL.createObjectURL(imageFile);
          this.uploadedImageId = result.value.imageId; // Assign the imageId
          this.resizedImageUrl = result.value.resizedImageUrl ?? null; // Store resized image URL
          this.detectedFaces = result.value.detectedFaces.map((face) => ({
            id: face.id,
            boundingBox: face.boundingBox,
            thumbnail: face.thumbnail, // Assign backend thumbnail (base64) to frontend thumbnail
            thumbnailUrl: face.thumbnailUrl, // Assign backend thumbnailUrl (public URL) to frontend thumbnailUrl
            memberId: face.memberId,
            originalMemberId: face.memberId, // Store the original memberId
            memberName: face.memberName,
            familyId: face.familyId,
            familyName: face.familyName,
            birthYear: face.birthYear,
            deathYear: face.deathYear,
            embedding: face.embedding, // Include embedding
            emotion: face.emotion,
            emotionConfidence: face.emotionConfidence,
            status: face.status || (face.memberId ? 'original-recognized' : 'unrecognized'), // Use backend status or derive
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

    // Action to select a face for labeling
    selectFace(faceId: string | undefined): void {
      this.selectedFaceId = faceId;
    },

    // Action to label a detected face with a member ID locally
    async labelFace(
      faceId: string,
      memberId: string,
      memberDetails: Member,
    ): Promise<void> {
      const faceIndex = this.detectedFaces.findIndex((f) => f.id === faceId);
      if (faceIndex !== -1) {
        this.detectedFaces[faceIndex].memberId = memberId;
        this.detectedFaces[faceIndex].status = 'labeled'; // Set status to labeled
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

    // Action to remove a detected face
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
  }, // This closes the actions object
}); // This closes the defineStore call

