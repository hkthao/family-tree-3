export interface RelationshipDetectionResult {
  description: string; // Add this property
  fromAToB?: string; // Make optional
  fromBToA?: string; // Make optional
  path: string[];
  edges: string[];
}
