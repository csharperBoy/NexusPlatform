
// modules/identity/models/UserDto.ts
export interface UserDto {
  userName: string;
  id: string ;
  phoneNumber: string | null;
  firstName?: string | null;
  lastName?: string | null;
  email?: string | null;
}
