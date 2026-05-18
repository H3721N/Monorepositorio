import { useState } from "react";
import { useAuth } from "../auth/AuthContext";
import type { ViewKey } from "../routes/views";
import { roleLabel } from "../utils/roles";

interface AppShellProps {
  currentView: ViewKey;
  onNavigate: (view: ViewKey) => void;
  children: React.ReactNode;
}

export function AppShell({ currentView, onNavigate, children }: AppShellProps) {
  const { user, logout, hasRole } = useAuth();
  const [isLoggingOut, setIsLoggingOut] = useState(false);

  const navItems: Array<{ key: ViewKey; label: string; visible: boolean }> = [
    { key: "dashboard", label: "Panel", visible: true },
    { key: "countries", label: "Países", visible: hasRole("COUNTRY") },
    { key: "departments", label: "Departamentos", visible: hasRole("DEPARTMENT") },
    { key: "users", label: "Usuarios", visible: hasRole("USER_ADMIN") },
    { key: "account", label: "Cuenta", visible: true }
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
              <button
                key={item.key}
                className={currentView === item.key ? "nav-item active" : "nav-item"}
                onClick={() => onNavigate(item.key)}
              >
                {item.label}
              </button>
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
        <main className="content">{children}</main>
      </div>
    </div>
  );
}
