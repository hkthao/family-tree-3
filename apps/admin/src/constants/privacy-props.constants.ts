// apps/admin/src/constants/privacy-props.constants.ts

/**
 * Các hằng số định nghĩa các thuộc tính có thể cấu hình quyền riêng tư cho các thực thể khác nhau.
 * Được sử dụng để xác định các trường dữ liệu mà người dùng có thể chọn để hiển thị công khai hoặc giữ riêng tư.
 */

/**
 * Thuộc tính của thành viên (Member).
 */
export const MEMBER_PROPS = {
  PROPERTIES: [
    'lastName',
    'firstName',
    'nickname',
    'gender',
    'dateOfBirth',
    'dateOfDeath',
    'placeOfBirth',
    'placeOfDeath',
    'occupation',
    'biography',
    'education',
    'religion',
    'phoneNumber',
    'email',
    'address',
    'identification',
    'socialMedia',
    'notes',
    'fatherId',
    'motherId',
    'husbandId',
    'wifeId',
  ],
};

/**
 * Thuộc tính của gia đình (Family).
 */
export const FAMILY_PROPS = {
  PROPERTIES: [
    'name',
    'description',
    'address',
    'totalMembers',
    'totalGenerations',
    'visibility',
    'avatarUrl',
    // 'familyUsers' and 'familyLimitConfiguration' are complex objects and not directly handled by simple checkboxes
  ],
};

/**
 * Thuộc tính của sự kiện (Event).
 */
export const EVENT_PROPS = {
  PROPERTIES: [
    'name',
    'code',
    'description',
    'calendarType',
    'solarDate',
    'lunarDate',
    'repeatRule',
    'type',
    'color',
    'familyId',
    'familyName',
    'familyAvatarUrl',
    // 'relatedMembers' (complex type)
    // 'relatedMemberIds' (derived)
  ],
};

/**
 * Thuộc tính của vị trí gia đình (FamilyLocation).
 */
export const FAMILY_LOCATION_PROPS = {
  PROPERTIES: [
    'name',
    'description',
    'latitude',
    'longitude',
    'address',
    'locationType',
    'accuracy',
    'source',
  ],
};

/**
 * Thuộc tính của mục kỷ niệm (MemoryItem).
 */
export const MEMORY_ITEM_PROPS = {
  PROPERTIES: [
    'title',
    'description',
    'happenedAt',
    'emotionalTag',
    // 'memoryMedia' (complex type)
    // 'memoryPersons' (complex type)
  ],
};

/**
 * Thuộc tính liên quan đến khuôn mặt thành viên (MemberFace).
 */
export const MEMBER_FACE_PROPS = {
  PROPERTIES: [
    'faceId',
    'confidence',
    'thumbnailUrl',
    'originalImageUrl',
    'emotion',
    'emotionConfidence',
    'isVectorDbSynced',
    'vectorDbId',
    'memberName',
    'memberGender',
    'memberAvatarUrl',
    'birthYear',
    'deathYear',
    'familyId',
    'familyName',
    'familyAvatarUrl',
    // 'boundingBox' (complex type)
    // 'embedding' (complex type)
  ],
};

/**
 * Thuộc tính liên quan đến khuôn mặt được tìm thấy (FoundFace).
 */
export const FOUND_FACE_PROPS = {
  PROPERTIES: [
    'faceId',
    'memberName',
    'score',
    'thumbnailUrl',
    'originalImageUrl',
    'emotion',
    'emotionConfidence',
    'familyAvatarUrl',
  ],
};