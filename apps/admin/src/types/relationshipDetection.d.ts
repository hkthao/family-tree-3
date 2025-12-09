export interface RelationshipDetectionResult {
  fromAToB: string;
  fromBToA: string;
  path: string[];
  edges: string[];
}
