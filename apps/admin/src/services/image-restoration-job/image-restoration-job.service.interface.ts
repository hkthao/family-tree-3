import { type Result, type Paginated, type ListOptions, type FilterOptions, type ImageRestorationJobDto, type CreateImageRestorationJobCommand, type UpdateImageRestorationJobCommand } from '@/types';

export interface IImageRestorationJobService {
  search(familyId: string, options?: ListOptions, filters?: FilterOptions): Promise<Result<Paginated<ImageRestorationJobDto>>>;
  getById(familyId: string, id: string): Promise<Result<ImageRestorationJobDto | undefined>>;
  add(newItem: CreateImageRestorationJobCommand): Promise<Result<ImageRestorationJobDto>>;
  update(updatedItem: UpdateImageRestorationJobCommand): Promise<Result<ImageRestorationJobDto>>;
  delete(familyId: string, id: string): Promise<Result<void>>;
}