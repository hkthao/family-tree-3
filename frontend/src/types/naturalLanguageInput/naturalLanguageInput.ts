export interface GenerateDataRequest {
  prompt: string;
}

export interface GeneratedDataResponse {
  jsonData: string;
  dataType: string;
}
