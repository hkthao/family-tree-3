import { type Result } from '@/types';
import { type DetectedFace } from '@/types';
import { type IFaceService } from './face.service.interface';

export class MockFaceService implements IFaceService {
  async detect(imageFile: File): Promise<Result<DetectedFace[], Error>> {
    // Simulate an API call delay
    await new Promise(resolve => setTimeout(resolve, 1000));

    // Simulate mock data for detected faces
    const mockDetectedFaces: DetectedFace[] = [
      { id: 'mock-face-1', boundingBox: { x: 50, y: 50, width: 100, height: 120 }, imageUrl: 'https://via.placeholder.com/100x120?text=Face1', memberId: null, status: 'unrecognized' },
      { id: 'mock-face-2', boundingBox: { x: 200, y: 80, width: 90, height: 110 }, imageUrl: 'https://via.placeholder.com/90x110?text=Face2', memberId: 'mock-member-1', status: 'recognized' },
    ];

    return { ok: true, value: mockDetectedFaces };
  }
}
