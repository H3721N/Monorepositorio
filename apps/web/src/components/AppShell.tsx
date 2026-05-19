import { useState } from "react";
import { NavLink, Outlet } from "react-router-dom";
import { useAuth } from "../auth/AuthContext";
import type { ViewKey } from "../routes/views";
import { roleLabel } from "../utils/roles";

export function AppShell() {
  const { user, logout, hasRole } = useAuth();
  const [isLoggingOut, setIsLoggingOut] = useState(false);

  const navItems: Array<{ key: ViewKey; to: string; label: string; visible: boolean }> = [
    { key: "dashboard", to: "/dashboard", label: "Panel", visible: true },
    { key: "countries", to: "/countries", label: "Países", visible: hasRole("COUNTRY") },
    { key: "departments", to: "/departments", label: "Departamentos", visible: hasRole("DEPARTMENT") },
    { key: "users", to: "/users", label: "Usuarios", visible: hasRole("USER_ADMIN") },
    { key: "account", to: "/account", label: "Cuenta", visible: true }
  ];

  async function handleLogout() {
    setIsLoggingOut(true);
    await logout();
    setIsLoggingOut(false);
  }

  return (
    <div className="app-shell">
      <aside className="sidebar">
        <div className="brand">
          <span className="brand-mark">M</span>
          <div>
            <strong>Monorepo</strong>
            <small>Administración</small>
          </div>
        </div>

        <nav className="nav-list">
          {navItems
            .filter((item) => item.visible)
            .map((item) => (
              <NavLink key={item.key} className={({ isActive }) => (isActive ? "nav-item active" : "nav-item")} to={item.to}>
                {item.label}
              </NavLink>
            ))}
        </nav>
      </aside>

      <div className="main-area">
        <header className="topbar">
          <div>
            <strong>{user?.email}</strong>
            <div className="role-list">{user?.roles.map(roleLabel).join(" · ")}</div>
          </div>
          <button className="secondary-button" disabled={isLoggingOut} onClick={handleLogout}>
            {isLoggingOut ? "Cerrando..." : "Cerrar sesión"}
          </button>
        </header>
        <main className="content">
          <Outlet />
        </main>
      </div>
    </div>
  );
}
