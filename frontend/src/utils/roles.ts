import type { RoleName } from "../types/api";

export const ROLE_OPTIONS: Array<{ id: number; name: RoleName; label: string }> = [
  { id: 4, name: "COUNTRY", label: "Países" },
  { id: 5, name: "DEPARTMENT", label: "Departamentos" },
  { id: 6, name: "USER_ADMIN", label: "Usuarios" }
];

export function roleLabel(role: RoleName): string {
  return ROLE_OPTIONS.find((option) => option.name === role)?.label ?? role;
}
