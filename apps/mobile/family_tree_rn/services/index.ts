// apps/mobile/family_tree_rn/services/index.ts

import { ApiUserProfileService } from '@/services/userProfile/api.userProfile.service';
import { ApiFamilyService } from '@/services/family/api.family.service';
import { IFamilyService } from '@/services/family/family.service.interface';
import { ApiMemberService } from '@/services/member/api.member.service'; // Import ApiMemberService
import { IMemberService } from '@/services/member/member.service.interface'; // Import IMemberService
import { ApiEventService } from '@/services/event/api.event.service'; // Import ApiEventService
import { IEventService } from '@/services/event/event.service.interface'; // Import IEventService
import { ApiRelationshipService } from '@/services/relationship/api.relationship.service'; // Import ApiRelationshipService
import { IRelationshipService } from '@/services/relationship/relationship.service.interface'; // Import IRelationshipService
import { ApiFaceService } from '@/services/face/api.face.service'; // Import ApiFaceService
import { IFaceService } from '@/services/face/face.service.interface'; // Import IFaceService
import { ApiFamilyDictService } from '@/services/familyDict/api.familyDict.service'; // Import ApiFamilyDictService
import { IFamilyDictService } from '@/services/familyDict/familyDict.service.interface'; // Import IFamilyDictService
import { ApiDashboardService } from '@/services/dashboard/api.dashboard.service'; // Import ApiDashboardService
import { IDashboardService } from '@/services/dashboard/dashboard.service.interface'; // Import IDashboardService
import { apiClientWithAuth, publicApiClient } from '@/services/api/publicApiClient'; // Import apiClientWithAuth and publicApiClient
import { IUserProfileService } from '@/services/userProfile/userProfile.service.interface';

// Initialize services
export const userProfileService: IUserProfileService = new ApiUserProfileService(apiClientWithAuth);
export const familyService: IFamilyService = new ApiFamilyService(publicApiClient);
export const memberService: IMemberService = new ApiMemberService(publicApiClient);
export const eventService: IEventService = new ApiEventService(publicApiClient);
export const relationshipService: IRelationshipService = new ApiRelationshipService(publicApiClient);
export const faceService: IFaceService = new ApiFaceService(publicApiClient);
export const familyDictService: IFamilyDictService = new ApiFamilyDictService(publicApiClient);
export const dashboardService: IDashboardService = new ApiDashboardService(publicApiClient);
