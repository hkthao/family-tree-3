import { type ImageRestorationJobDto, type CreateImageRestorationJobDto, type UpdateImageRestorationJobDto } from '@/types';
import { type ICrudService } from '../common/crud.service.interface';

export interface IImageRestorationJobService extends ICrudService<ImageRestorationJobDto, CreateImageRestorationJobDto, UpdateImageRestorationJobDto> {
  // search method will implicitly handle familyId via FilterOptions
  // getById and delete methods are used as defined in ICrudService
}