import { useQueryClient } from "@tanstack/react-query";
import { createContext, useCallback, useContext, useEffect, useMemo, useState } from "react";
import { authApi, userApi } from "../services/api/endpoints";
import { ApiError, setUnauthorizedHandler } from "../services/api/httpClient";
import { clearStoredTokens, getStoredTokens, setStoredTokens } from "../services/api/tokenStorage";
import type { RoleName, User } from "../types/api";
import { useAuthStore } from "./authStore";

interface AuthContextValue {
  user: User | null;
  roles: RoleName[];
  isAuthenticated: boolean;
  isInitializing: boolean;
  login: (email: string, password: string) => Promise<void>;
  logout: () => Promise<void>;
  refreshCurrentUser: () => Promise<void>;
  hasRole: (role: RoleName) => boolean;
}

const AuthContext = createContext<AuthContextValue | null>(null);

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const queryClient = useQueryClient();
  const user = useAuthStore((state) => state.user);
  const setUser = useAuthStore((state) => state.setUser);
  const clearUser = useAuthStore((state) => state.clearUser);
  const [isInitializing, setIsInitializing] = useState(true);

  const clearSession = useCallback(() => {
    clearStoredTokens();
    clearUser();
    queryClient.removeQueries({ queryKey: ["auth", "me"] });
  }, [clearUser, queryClient]);

  const refreshCurrentUser = useCallback(async () => {
    const currentUser = await queryClient.fetchQuery({
      queryKey: ["auth", "me"],
      queryFn: userApi.me,
      staleTime: 60_000
    });
    setUser(currentUser);
  }, [queryClient, setUser]);

  const login = useCallback(
    async (email: string, password: string) => {
      const tokens = await authApi.login(email, password);
      setStoredTokens(tokens);
      await refreshCurrentUser();
    },
    [refreshCurrentUser]
  );

  const logout = useCallback(async () => {
    try {
      if (getStoredTokens()) {
        await authApi.logout();
      }
    } catch (error) {
      if (!(error instanceof ApiError && error.status === 401)) {
        throw error;
      }
    } finally {
      clearSession();
    }
  }, [clearSession]);

  useEffect(() => {
    setUnauthorizedHandler(clearSession);
    return () => setUnauthorizedHandler(null);
  }, [clearSession]);

  useEffect(() => {
    async function initialize() {
      try {
        if (getStoredTokens()) {
          await refreshCurrentUser();
        }
      } catch {
        clearSession();
      } finally {
        setIsInitializing(false);
      }
    }

    initialize();
  }, [clearSession, refreshCurrentUser]);

  const value = useMemo<AuthContextValue>(() => {
    const roles = user?.roles ?? [];
    return {
      user,
      roles,
      isAuthenticated: Boolean(user),
      isInitializing,
      login,
      logout,
      refreshCurrentUser,
      hasRole: (role) => roles.includes(role)
    };
  }, [isInitializing, login, logout, refreshCurrentUser, user]);

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth(): AuthContextValue {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error("useAuth must be used inside AuthProvider.");
  }

  return context;
}
