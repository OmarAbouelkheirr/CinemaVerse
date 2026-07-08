export type IsoDateTimeString = string;
export type IsoDateString = string;

export enum UserGender {
  Male = 'Male',
  Female = 'Female',
}

export enum UserRole {
  Admin = 'Admin',
  User = 'User',
  RegularUser = 'RegularUser',
}

export interface UserIdentity {
  userId: number;
  id: number;
  email: string;
  role: UserRole;
}

export interface UserProfilePersonalInfo {
  firstName: string;
  lastName: string;
  phoneNumber: string;
  address: string;
  city: string;
  dateOfBirth: IsoDateTimeString;
  gender: UserGender;
}

export interface UserProfileMeta {
  isEmailConfirmed: boolean;
  createdAt: IsoDateTimeString;
}

export interface UserProfile extends UserIdentity, UserProfilePersonalInfo, UserProfileMeta {}

export type GetMyProfileResponse = UserProfile;

export interface UpdateProfileRequest {
  firstName: string;
  lastName: string;
  phoneNumber: string;
  address: string;
  city: string;
  dateOfBirth: IsoDateString;
  gender: UserGender;
}

export interface ChangePasswordRequest {
  currentPassword: string;
  newPassword: string;
}
