import type { MemberStoryDto } from '@/types/memberStory.d'; 
import type { ICrudService } from '../common/crud.service.interface';

export interface IMemberStoryService extends ICrudService<MemberStoryDto> { 
}