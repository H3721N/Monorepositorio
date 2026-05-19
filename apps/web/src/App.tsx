import { Navigate, Outlet, Route, Routes } from "react-router-dom";
import { useAuth } from "./auth/AuthContext";
import { AppShell } from "./components/AppShell";
import { StatusMessage } from "./components/StatusMessage";
import { AccountPage } from "./pages/AccountPage";
import { CountriesPage } from "./pages/CountriesPage";
import { DashboardPage } from "./pages/DashboardPage";
import { DepartmentsPage } from "./pages/DepartmentsPage";
import { LoginPage } from "./pages/LoginPage";
import { UsersPage } from "./pages/UsersPage";
import type { RoleName } from "./types/api";

export function App() {
  const { isAuthenticated, isInitializing } = useAuth();

  if (isInitializing) {
    return (
      <main className="boot-screen">
        <div className="loader" />
        <p>Preparando sesión...</p>
      </main>
    );
  }

  return (
    <Routes>
      <Route path="/login" element={isAuthenticated ? <Navigate to="/dashboard" replace /> : <LoginPage />} />
      <Route element={<ProtectedRoute />}>
        <Route element={<AppShell />}>
          <Route index element={<Navigate to="/dashboard" replace />} />
          <Route path="/dashboard" element={<DashboardPage />} />
          <Route path="/countries" element={<RoleRoute role="COUNTRY"><CountriesPage /></RoleRoute>} />
          <Route path="/departments" element={<RoleRoute role="DEPARTMENT"><DepartmentsPage /></RoleRoute>} />
          <Route path="/users" element={<RoleRoute role="USER_ADMIN"><UsersPage /></RoleRoute>} />
          <Route path="/account" element={<AccountPage />} />
        </Route>
      </Route>
      <Route path="*" element={<Navigate to={isAuthenticated ? "/dashboard" : "/login"} replace />} />
    </Routes>
  );
}

function ProtectedRoute() {
  const { isAuthenticated } = useAuth();
  return isAuthenticated ? <Outlet /> : <Navigate to="/login" replace />;
}

function RoleRoute({ role, children }: { role: RoleName; children: React.ReactNode }) {
  const { hasRole } = useAuth();
  return hasRole(role) ? children : <StatusMessage type="error">No tienes permiso para esta vista.</StatusMessage>;
}
