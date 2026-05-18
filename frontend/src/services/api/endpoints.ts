import type {
  Country,
  CountryDetail,
  CreateCountryPayload,
  CreateDepartmentPayload,
  CreateUserPayload,
  Department,
  TokenResponse,
  UpdateCountryPayload,
  UpdateDepartmentPayload,
  UpdateUserRolesPayload,
  User
} from "../../types/api";
import { apiRequest } from "./httpClient";

export const authApi = {
  login: (email: string, password: string) =>
    apiRequest<TokenResponse>("/api/Auth/login", {
      method: "POST",
      skipAuth: true,
      body: JSON.stringify({ email, password })
    }),
  logout: () =>
    apiRequest<void>("/api/Auth/logout", {
      method: "POST"
    }),
  changePassword: (currentPassword: string, newPassword: string) =>
    apiRequest<void>("/api/Auth/change-password", {
      method: "POST",
      body: JSON.stringify({ currentPassword, newPassword })
    })
};

export const userApi = {
  me: () => apiRequest<User>("/api/admin/users/me"),
  create: (payload: CreateUserPayload) =>
    apiRequest<User>("/api/admin/users", {
      method: "POST",
      body: JSON.stringify(payload)
    }),
  deactivate: (id: number) =>
    apiRequest<void>(`/api/admin/users/${id}`, {
      method: "DELETE"
    }),
  updateRoles: (id: number, payload: UpdateUserRolesPayload) =>
    apiRequest<User>(`/api/admin/users/${id}/roles`, {
      method: "PUT",
      body: JSON.stringify(payload)
    })
};

export const countriesApi = {
  list: () => apiRequest<Country[]>("/api/Countries"),
  detail: (id: number) => apiRequest<CountryDetail>(`/api/Countries/${id}`),
  create: (payload: CreateCountryPayload) =>
    apiRequest<Country>("/api/Countries", {
      method: "POST",
      body: JSON.stringify(payload)
    }),
  update: (id: number, payload: UpdateCountryPayload) =>
    apiRequest<Country>(`/api/Countries/${id}`, {
      method: "PUT",
      body: JSON.stringify(payload)
    }),
  remove: (id: number) =>
    apiRequest<void>(`/api/Countries/${id}`, {
      method: "DELETE"
    })
};

export const departmentsApi = {
  list: (countryId?: number) =>
    apiRequest<Department[]>(countryId ? `/api/Departments?countryId=${countryId}` : "/api/Departments"),
  create: (payload: CreateDepartmentPayload) =>
    apiRequest<Department>("/api/Departments", {
      method: "POST",
      body: JSON.stringify(payload)
    }),
  update: (id: number, payload: UpdateDepartmentPayload) =>
    apiRequest<Department>(`/api/Departments/${id}`, {
      method: "PUT",
      body: JSON.stringify(payload)
    }),
  remove: (id: number) =>
    apiRequest<void>(`/api/Departments/${id}`, {
      method: "DELETE"
    })
};
