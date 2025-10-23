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
  approved: boolean;
}
