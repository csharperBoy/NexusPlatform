import { PersonDto } from "@/modules/HR/models/PersonDto";

// modules/identity/models/UserDto.ts
export interface UserDto {
  userName: string;
  id: string ;
  phoneNumber: string | null;
  nickName?: string | null;
  email?: string | null;
  person?: PersonDto | null;
}
