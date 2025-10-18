import { defineStore } from 'pinia';
import type { DetectedFace, SearchResult, Member } from '@/types';
import i18n from '@/plugins/i18n'; // For localization

interface FaceState {
  uploadedImage: string | null; // Base64 or URL of the uploaded image
  uploadedImageId: string | null; // ID of the uploaded image from the backend
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
    async detectFaces(imageFile: File): Promise<void> {
      this.loading = true;
      this.error = null;
      try {
        const result = await this.services.face.detect(imageFile);
        if (result.ok) {
          this.uploadedImage = URL.createObjectURL(imageFile);
          this.uploadedImageId = result.value.imageId; // Assign the imageId
          this.detectedFaces = result.value.detectedFaces.map((face) => ({
            id: face.id,
            boundingBox: face.boundingBox,
            thumbnail: face.thumbnail,
            memberId: face.memberId,
            memberName: face.memberName,
            familyId: face.familyId,
            familyName: face.familyName,
            birthYear: face.birthYear,
            deathYear: face.deathYear,
            embedding: face.embedding, // Include embedding
            status: face.memberId ? 'recognized' : 'unrecognized',
          }));
        } else {
          this.error =
            result.error?.message ||
            i18n.global.t('face.errors.detectionFailed');
        }
      } catch (err: any) {
        this.error =
          err.message || i18n.global.t('face.errors.unexpectedError');
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

    // Action to remove a detected face
    removeFace(faceId: string): void {
      this.detectedFaces = this.detectedFaces.filter(
        (face) => face.id !== faceId,
      );
    },

    // Action to save all face labels to the backend
    async saveFaceLabels(): Promise<boolean> {
      this.loading = true;
      this.error = null;
      try {
        // Prepare data for API call
        const faceLabels = this.detectedFaces
          .filter((face) => face.memberId)
          .map((face) => ({
            id: face.id,
            boundingBox: face.boundingBox,
            thumbnail: face.thumbnail,
            memberId: face.memberId,
            memberName: face.memberName,
            familyId: face.familyId,
            familyName: face.familyName,
            birthYear: face.birthYear,
            deathYear: face.deathYear,
            embedding: face.embedding, // Include embedding
            status: face.status, // Include status
          }));

        // Assuming imageId is stored somewhere, e.g., in the store state or passed as a prop
        // For now, let's assume it's available in the store as uploadedImageId
        const imageId = this.uploadedImageId; // You need to add uploadedImageId to your state

        if (!imageId) {
          throw new Error('Image ID is missing. Cannot save face labels.');
        }

        const result = await this.services.face.saveLabels(faceLabels, imageId);

        if (result.ok) {
          // Optionally update status of faces after successful save
          this.detectedFaces.forEach((face) => {
            if (face.memberId) face.status = 'recognized';
          });
          return true;
        } else {
          this.error =
            result.error?.message ||
            i18n.global.t('face.errors.saveMappingFailed');
          return false;
        }
      } catch (err: any) {
        this.error =
          err.message || i18n.global.t('face.errors.unexpectedError');
        return false;
      } finally {
        this.loading = false;
      }
    },

    // Action to search for members by a single face image
    async searchByFace(imageFile: File): Promise<void> {
      this.loading = true;
      this.error = null;
      this.faceSearchResults = [];
      try {
        // Simulate API call
        // const result: Result<SearchResult[], Error> = await this.services.face.search(imageFile);
        // For now, simulate with mock data
        const mockSearchResults: SearchResult[] = [
          {
            member: {
              id: 'member123',
              fullName: 'John Doe',
              avatarUrl: 'path/to/john.jpg',
            },
            confidence: 0.95,
          },
          {
            member: {
              id: 'member456',
              fullName: 'Jane Smith',
              avatarUrl: 'path/to/jane.jpg',
            },
            confidence: 0.8,
          },
        ];
        this.faceSearchResults = mockSearchResults; // Replace with actual API result

        // if (result.ok) {
        //   this.faceSearchResults = result.value;
        // } else {
        //   this.error = result.error?.message || i18n.global.t('face.errors.searchFailed');
        // }
      } catch (err: any) {
        this.error =
          err.message || i18n.global.t('face.errors.unexpectedError');
      } finally {
        this.loading = false;
      }
    },

    // Reset the store state
    resetState(): void {
      this.uploadedImage = null;
      this.detectedFaces = [];
      this.selectedFaceId = undefined;
      this.faceSearchResults = [];
      this.loading = false;
      this.error = null;
    },
  },
});
