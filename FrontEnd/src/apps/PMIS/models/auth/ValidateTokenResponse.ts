// src/models/auth/ValidateTokenResponse.ts
export interface ValidateTokenResponse {
  isValid: boolean;
  userId: string;
  roles: string[];
}
