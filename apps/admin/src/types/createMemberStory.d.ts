import type { DetectedFace } from './face.d'; // Assuming DetectedFace is defined here

// Matches backend.Application.MemberStories.Commands.CreateMemberStory.CreateMemberStoryCommand
export interface CreateMemberStory {
  memberId: string;
  title: string;
  story: string;
  originalImageUrl?: string | null;
  resizedImageUrl?: string | null;
  rawInput?: string | null; // NEW
  storyStyle?: string | null; // NEW
  perspective?: string | null; // NEW
  detectedFaces: DetectedFace[]; // Using the aligned frontend DTO
}
