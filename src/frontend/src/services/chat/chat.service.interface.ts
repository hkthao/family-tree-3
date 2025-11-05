import type { ApiError } from "@/plugins/axios";
import type { Result } from "@/types";

export interface IChatService {
  sendMessage(message: string): Promise<Result<string, ApiError>>;
  sendMessageStream(message: string): AsyncGenerator<string, void, unknown>;
}