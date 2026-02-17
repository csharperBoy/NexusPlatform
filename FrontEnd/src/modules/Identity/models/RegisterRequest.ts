
// modules/identity/models/RegisterRequest.ts
export interface RegisterRequest {
  username: string;
  email: string;
  password: string;
  displayName?: string;
}