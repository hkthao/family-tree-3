import { type ApiClientMethods } from '@/plugins/axios';
import { type Result, type Paginated, type ListOptions, type FilterOptions, type ImageRestorationJobDto, type CreateImageRestorationJobDto, type UpdateImageRestorationJobDto } from '@/types';
import { type IImageRestorationJobService } from './image-restoration-job.service.interface';
import { ApiCrudService } from '../common/api.crud.service';

export class ApiImageRestorationJobService
  extends ApiCrudService<ImageRestorationJobDto, CreateImageRestorationJobDto, UpdateImageRestorationJobDto>
  implements IImageRestorationJobService
{
  constructor(protected http: ApiClientMethods) {
    super(http, '/image-restoration-jobs');
  }

  // Override search to include familyId in the filters parameter for ApiCrudService.search
  async search(
    options?: ListOptions,
    filters?: FilterOptions,
  ): Promise<Result<Paginated<ImageRestorationJobDto>>> {
    return await super.search(options, filters);
  }

  // getById, add, update, delete methods will now use the base ApiCrudService implementation
  // as the backend ImageRestorationJobsController does not expect familyId in route/query for these.
}