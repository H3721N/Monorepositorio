export type RoleName = "COUNTRY" | "DEPARTMENT" | "USER_ADMIN";

export interface TokenResponse {
  accessToken: string;
  refreshToken: string;
  accessTokenExpiresAt: string;
  refreshTokenExpiresAt: string;
}

export interface User {
  id: number;
  email: string;
  roles: RoleName[];
  activo: boolean;
}

export interface Country {
  id: number;
  name: string;
  isoCode: string;
}

export interface CountryDetail extends Country {
  departments: Department[];
}

export interface Department {
  id: number;
  name: string;
  countryId: number;
}

export interface CreateCountryPayload {
  name: string;
  isoCode: string;
}

export interface UpdateCountryPayload extends CreateCountryPayload {}

export interface CreateDepartmentPayload {
  name: string;
  countryId: number;
}

export interface UpdateDepartmentPayload extends CreateDepartmentPayload {}

export interface CreateUserPayload {
  email: string;
  password: string;
  roleIds: number[];
}

export interface UpdateUserRolesPayload {
  roleIds: number[];
}

export interface ApiErrorBody {
  errors?: string[];
  title?: string;
  detail?: string;
}
