import { type ApiClientMethods } from '@/plugins/axios';
import { type Result, type Paginated, type ListOptions, type FilterOptions, type ImageRestorationJobDto, type CreateImageRestorationJobCommand, type UpdateImageRestorationJobCommand } from '@/types';
import { type IImageRestorationJobService } from './image-restoration-job.service.interface';
import { buildSearchParams } from '@/utils/list.utils';

export class ApiImageRestorationJobService implements IImageRestorationJobService {
  private readonly RESOURCE_BASE_URL = '/api/image-restoration-jobs';

  constructor(protected http: ApiClientMethods) {}

  async search(
    familyId: string,
    options?: ListOptions,
    filters?: FilterOptions,
  ): Promise<Result<Paginated<ImageRestorationJobDto>>> {
    const params: Record<string, any> = {
      ...buildSearchParams(options, filters),
      familyId: familyId, // Add familyId to query params
    };
    return await this.http.get<Paginated<ImageRestorationJobDto>>(`${this.RESOURCE_BASE_URL}/search`, { params });
  }

  async getById(familyId: string, id: string): Promise<Result<ImageRestorationJobDto | undefined>> {
    return await this.http.get<ImageRestorationJobDto>(`${this.RESOURCE_BASE_URL}/${id}?familyId=${familyId}`);
  }

  async add(newItem: CreateImageRestorationJobCommand): Promise<Result<ImageRestorationJobDto>> {
    return await this.http.post<ImageRestorationJobDto>(this.RESOURCE_BASE_URL, newItem);
  }

  async update(updatedItem: UpdateImageRestorationJobCommand): Promise<Result<ImageRestorationJobDto>> {
    return await this.http.put<ImageRestorationJobDto>(`${this.RESOURCE_BASE_URL}/${updatedItem.jobId}`, updatedItem);
  }

  async delete(familyId: string, id: string): Promise<Result<void>> {
    return await this.http.delete<void>(`${this.RESOURCE_BASE_URL}/${id}?familyId=${familyId}`);
  }
}