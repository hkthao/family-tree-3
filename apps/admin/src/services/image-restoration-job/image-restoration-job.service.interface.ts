import { type ImageRestorationJobDto, type UpdateImageRestorationJobDto, type Result } from '@/types';
import { type ICrudService } from '../common/crud.service.interface';

// Define a new interface that extends ICrudService but overrides the add method
export interface IImageRestorationJobService extends Omit<ICrudService<ImageRestorationJobDto, any, UpdateImageRestorationJobDto>, 'add'> {
  // Override the add method to handle file uploads
  add(file: File, familyId: string, useCodeformer: boolean): Promise<Result<ImageRestorationJobDto>>;
}