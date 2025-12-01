// apps/admin/src/types/n8n.d.ts

export interface GenerateWebhookJwtResponse {
    token: string;
    expiresAt: string; // ISO 8601 date string
}
