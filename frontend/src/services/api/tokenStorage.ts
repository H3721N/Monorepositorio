import type { TokenResponse } from "../../types/api";

const STORAGE_KEY = "monorepo.tokens";

export function getStoredTokens(): TokenResponse | null {
  const raw = localStorage.getItem(STORAGE_KEY);
  if (!raw) {
    return null;
  }

  try {
    return JSON.parse(raw) as TokenResponse;
  } catch {
    localStorage.removeItem(STORAGE_KEY);
    return null;
  }
}

export function setStoredTokens(tokens: TokenResponse): void {
  localStorage.setItem(STORAGE_KEY, JSON.stringify(tokens));
}

export function clearStoredTokens(): void {
  localStorage.removeItem(STORAGE_KEY);
}
