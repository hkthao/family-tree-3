export enum LifeStage {
  Childhood = 1,
  Adulthood = 2,
  StartingAFamily = 3,
  SignificantEvents = 4,
  OldAge = 5,
  Deceased = 6,
}

export enum FamilyRole {
  Viewer = 1,
  Manager = 2,
  Admin = 3, // Global admin, not family specific
}

export enum RefType {
  Member = 0,
  MemberStory = 1,
  // Add other reference types as needed
}

export enum MediaType {
  Image = 1,
  Video = 2,
  Audio = 3,
  Document = 4,
  Other = 5,
}