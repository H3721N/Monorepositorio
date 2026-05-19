import "@testing-library/jest-dom/vitest";
import { cleanup } from "@testing-library/react";
import { afterEach, beforeEach, vi } from "vitest";
import { useAuthStore } from "../auth/authStore";

beforeEach(() => {
  localStorage.clear();
  vi.restoreAllMocks();
  useAuthStore.getState().clearUser();
});

afterEach(() => {
  cleanup();
});
