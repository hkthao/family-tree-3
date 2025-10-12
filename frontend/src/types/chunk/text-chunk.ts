export interface TextChunk {
  id: string;
  content: string;
  metadata: {
    fileName: string;
    fileId: string;
    familyId: string;
    page: string;
    category: string;
    createdBy: string;
    createdAt: string;
  };
  approved?: boolean; // Add an approved status for UI interaction
}
