export interface GenerateVoiceCommand {
  voiceProfileId: string;
  text: string;
}

export interface VoiceGenerationDto {
  id: string;
  voiceProfileId: string;
  text: string;
  audioUrl: string;
  durationSeconds: number;
  created: string;
  createdBy?: string;
}
