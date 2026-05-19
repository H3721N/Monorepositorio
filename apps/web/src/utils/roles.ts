import type { RoleName } from "../types/api";

export const ROLE_OPTIONS: Array<{ id: number; name: RoleName; label: string }> = [
  { id: 1, name: "COUNTRY", label: "Países" },
  { id: 2, name: "DEPARTMENT", label: "Departamentos" },
  { id: 3, name: "USER_ADMIN", label: "Usuarios" }
];

export function roleLabel(role: RoleName): string {
  return ROLE_OPTIONS.find((option) => option.name === role)?.label ?? role;
}
