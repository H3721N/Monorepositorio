import { useEffect, useMemo, useState } from "react";
import { useAuth } from "./auth/AuthContext";
import { AppShell } from "./components/AppShell";
import { StatusMessage } from "./components/StatusMessage";
import { AccountPage } from "./pages/AccountPage";
import { CountriesPage } from "./pages/CountriesPage";
import { DashboardPage } from "./pages/DashboardPage";
import { DepartmentsPage } from "./pages/DepartmentsPage";
import { LoginPage } from "./pages/LoginPage";
import { UsersPage } from "./pages/UsersPage";
import type { ViewKey } from "./routes/views";

export function App() {
  const { isAuthenticated, isInitializing, hasRole } = useAuth();
  const [currentView, setCurrentView] = useState<ViewKey>("dashboard");

  const allowedViews = useMemo<ViewKey[]>(() => {
    const views: ViewKey[] = ["dashboard", "account"];
    if (hasRole("COUNTRY")) {
      views.push("countries");
    }
    if (hasRole("DEPARTMENT")) {
      views.push("departments");
    }
    if (hasRole("USER_ADMIN")) {
      views.push("users");
    }
    return views;
  }, [hasRole]);

  useEffect(() => {
    if (!allowedViews.includes(currentView)) {
      setCurrentView("dashboard");
    }
  }, [allowedViews, currentView]);

  if (isInitializing) {
    return (
      <main className="boot-screen">
        <div className="loader" />
        <p>Preparando sesión...</p>
      </main>
    );
  }

  if (!isAuthenticated) {
    return <LoginPage />;
  }

  function renderView() {
    if (!allowedViews.includes(currentView)) {
      return <StatusMessage type="error">No tienes permiso para esta vista.</StatusMessage>;
    }

    switch (currentView) {
      case "countries":
        return <CountriesPage />;
      case "departments":
        return <DepartmentsPage />;
      case "users":
        return <UsersPage />;
      case "account":
        return <AccountPage />;
      default:
        return <DashboardPage />;
    }
  }

  return (
    <AppShell currentView={currentView} onNavigate={setCurrentView}>
      {renderView()}
    </AppShell>
  );
}
