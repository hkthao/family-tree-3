export enum NotificationChannel {
  InApp = 0,
  Firebase = 1,
  Email = 2,
  SMS = 3,
  Webhook = 4,
}

export enum NotificationType {
  General = 0,
  FamilyCreated = 1,
  FamilyUpdated = 2,
  FamilyDeleted = 3,
  MemberCreated = 4,
  MemberUpdated = 5,
  MemberDeleted = 6,
  MemberBiographyUpdated = 7,
  NewFamilyMember = 8,
  RelationshipCreated = 9,
  RelationshipUpdated = 10,
  RelationshipDeleted = 11,
}
