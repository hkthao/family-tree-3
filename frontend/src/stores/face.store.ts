import { defineStore } from 'pinia';
import type { DetectedFace, SearchResult } from '@/types';
import i18n from '@/plugins/i18n'; // For localization

interface FaceState {
  uploadedImage: string | null; // Base64 or URL of the uploaded image
  detectedFaces: DetectedFace[]; // Array of detected faces with bounding boxes
  selectedFaceId: string | null; // ID of the currently selected face for labeling
  faceSearchResults: SearchResult[]; // Results from face search
  loading: boolean;
  error: string | null;
}

export const useFaceStore = defineStore('face', {
  state: (): FaceState => ({
    uploadedImage: null,
    detectedFaces: [],
    selectedFaceId: null,
    faceSearchResults: [],
    loading: false,
    error: null,
  }),

  getters: {
    // Getters to retrieve specific data from the state
    currentSelectedFace: (state) => state.detectedFaces.find(face => face.id === state.selectedFaceId),
    unlabeledFaces: (state) => state.detectedFaces.filter(face => !face.memberId),
    labeledFaces: (state) => state.detectedFaces.filter(face => face.memberId),
  },

  actions: {
    // Action to handle image upload and face detection
    async detectFaces(imageFile: File): Promise<void> {
      this.loading = true;
      this.error = null;
      try {
        // Simulate API call
        // const result: Result<DetectedFace[], Error> = await this.services.face.detect(imageFile);
        // For now, simulate with mock data
        const mockDetectedFaces: DetectedFace[] = [
          { id: 'face1', boundingBox: { x: 10, y: 20, width: 50, height: 60 }, imageUrl: 'path/to/cropped/face1.jpg', memberId: null, status: 'unrecognized' },
          { id: 'face2', boundingBox: { x: 70, y: 80, width: 45, height: 55 }, imageUrl: 'path/to/cropped/face2.jpg', memberId: 'member123', status: 'recognized' },
          // ... more mock faces
        ];
        this.uploadedImage = URL.createObjectURL(imageFile); // Store URL for display
        this.detectedFaces = mockDetectedFaces; // Replace with actual API result

        // if (result.ok) {
        //   this.uploadedImage = URL.createObjectURL(imageFile);
        //   this.detectedFaces = result.value.map(face => ({ ...face, status: face.memberId ? 'recognized' : 'unrecognized' }));
        // } else {
        //   this.error = result.error?.message || i18n.global.t('face.errors.detectionFailed');
        // }
      } catch (err: any) {
        this.error = err.message || i18n.global.t('face.errors.unexpectedError');
      } finally {
        this.loading = false;
      }
    },

    // Action to select a face for labeling
    selectFace(faceId: string | null): void {
      this.selectedFaceId = faceId;
    },

    // Action to save face mapping to a member
    async saveFaceMapping(faceId: string, memberId: string): Promise<void> {
      this.loading = true;
      this.error = null;
      try {
        // Simulate API call
        // const result: Result<void, Error> = await this.services.face.saveMapping(faceId, memberId);
        // For now, update local state
        const faceIndex = this.detectedFaces.findIndex(f => f.id === faceId);
        if (faceIndex !== -1) {
          this.detectedFaces[faceIndex].memberId = memberId;
          this.detectedFaces[faceIndex].status = 'newly-labeled'; // Or 'recognized'
        }

        // if (result.ok) {
        //   // Update the detected face in the state
        //   const faceIndex = this.detectedFaces.findIndex(f => f.id === faceId);
        //   if (faceIndex !== -1) {
        //     this.detectedFaces[faceIndex].memberId = memberId;
        //     this.detectedFaces[faceIndex].status = 'newly-labeled'; // Assuming API confirms
        //   }
        //   // Show success notification
        //   // this.notificationStore.showSnackbar(i18n.global.t('face.success.saveMapping'), 'success');
        // } else {
        //   this.error = result.error?.message || i18n.global.t('face.errors.saveMappingFailed');
        // }
      } catch (err: any) {
        this.error = err.message || i18n.global.t('face.errors.unexpectedError');
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
          { member: { id: 'member123', fullName: 'John Doe', avatarUrl: 'path/to/john.jpg' }, confidence: 0.95 },
          { member: { id: 'member456', fullName: 'Jane Smith', avatarUrl: 'path/to/jane.jpg' }, confidence: 0.80 },
        ];
        this.faceSearchResults = mockSearchResults; // Replace with actual API result

        // if (result.ok) {
        //   this.faceSearchResults = result.value;
        // } else {
        //   this.error = result.error?.message || i18n.global.t('face.errors.searchFailed');
        // }
      } catch (err: any) {
        this.error = err.message || i18n.global.t('face.errors.unexpectedError');
      } finally {
        this.loading = false;
      }
    },

    // Reset the store state
    resetState(): void {
      this.uploadedImage = null;
      this.detectedFaces = [];
      this.selectedFaceId = null;
      this.faceSearchResults = [];
      this.loading = false;
      this.error = null;
    },
  },
});
