export interface UpdateUserProfileRequestDto {
    id: string;
    externalId?: string;
    email: string;
    name: string;
    firstName?: string;
    lastName?: string;
    phone?: string;
    avatarBase64?: string | null; // Can be a base64 string, null (to clear), or undefined (no change)
}
