import { type ApiClientMethods } from '@/plugins/axios';
import { type Result, type Paginated, type ListOptions, type FilterOptions, type ImageRestorationJobDto, type UpdateImageRestorationJobDto } from '@/types';
import { type IImageRestorationJobService } from './image-restoration-job.service.interface';
import { ApiCrudService } from '../common/api.crud.service';

export class ApiImageRestorationJobService
  implements IImageRestorationJobService
{
  private crudService: ApiCrudService<ImageRestorationJobDto, never, UpdateImageRestorationJobDto>;
  protected baseUrl: string;

  constructor(protected http: ApiClientMethods) {
    this.baseUrl = '/image-restoration-jobs';
    this.crudService = new ApiCrudService<ImageRestorationJobDto, never, UpdateImageRestorationJobDto>(http, this.baseUrl);
  }

  // Override search to include familyId in the filters parameter for ApiCrudService.search
  async search(
    options?: ListOptions,
    filters?: FilterOptions,
  ): Promise<Result<Paginated<ImageRestorationJobDto>>> {
    return await this.crudService.search(options, filters);
  }

  async getById(id: string): Promise<Result<ImageRestorationJobDto | undefined>> {
    return await this.crudService.getById(id);
  }

  async update(updatedItem: UpdateImageRestorationJobDto): Promise<Result<ImageRestorationJobDto>> {
    return await this.crudService.update(updatedItem);
  }

  async delete(id: string): Promise<Result<void>> {
    return await this.crudService.delete(id);
  }

  async getByIds(ids: string[]): Promise<Result<ImageRestorationJobDto[]>> {
    return await this.crudService.getByIds(ids);
  }

  // Override add to handle file upload
  async add(
    file: File,
    familyId: string,
    useCodeformer: boolean,
  ): Promise<Result<ImageRestorationJobDto>> {
    const formData = new FormData();
    formData.append('imageFile', file);
    formData.append('familyId', familyId);
    formData.append('useCodeformer', useCodeformer.toString());

    // Use the http client directly for multipart/form-data
    return await this.http.post<ImageRestorationJobDto>(this.baseUrl, formData, {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
    });
  }

  // getById, update, delete methods will now use the base ApiCrudService implementation
  // as the backend ImageRestorationJobsController does not expect familyId in route/query for these.
}