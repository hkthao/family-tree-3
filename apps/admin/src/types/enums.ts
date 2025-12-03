export enum MemberStoryStyle {
  Nostalgic = 'nostalgic',
  Warm = 'warm',
  Formal = 'formal',
  Folk = 'folk',
}

export enum MemberStoryPerspective {
  FirstPerson = 'firstPerson',
  ThirdPerson = 'thirdPerson',
  FamilyMember = 'familyMember',
  NeutralPersonal = 'neutralPersonal', // Added from useMemberStoryForm suggestions
  FullyNeutral = 'fullyNeutral',     // Added from useMemberStoryForm suggestions
}

export enum FamilyRole {
  Viewer = 1,
  Manager = 2,
  Admin = 3, // Global admin, not family specific
}