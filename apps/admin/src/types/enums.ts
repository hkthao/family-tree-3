// apps/admin/src/types/enums.ts

export enum CalendarType {
  Solar = 1,
  Lunar = 2,
}

export enum RepeatRule {
  None = 0,
  Yearly = 1,
}

export enum EmotionalTag {
  Happy = 0,
  Sad = 1,
  Proud = 2,
  Memorial = 3,
  Neutral = 4,
}

export enum MediaType {
  Image = 0,
  Video = 1,
  Audio = 2,
  Document = 3,
  Other = 4
}

export enum RefType {
  Family = 0,
  Member = 1,
  Event = 2,
  FamilyMedia = 3,
  Relationship = 4
}

export enum LifeStage {
  Childhood = 0,
  Adolescence = 1,
  Adulthood = 2,
  Elderly = 3,
  StartingAFamily = 4, // Added to match frontend usage
  SignificantEvents = 5, // Added to match frontend usage
  OldAge = 6, // Added to match frontend usage
  Deceased = 7 // Added to match frontend usage
}

export enum FamilyRole {
  Manager = 0,
  Editor = 1,
  Viewer = 2
}
