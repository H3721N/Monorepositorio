import type { ApiErrorBody, TokenResponse } from "../../types/api";
import { clearStoredTokens, getStoredTokens, setStoredTokens } from "./tokenStorage";

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? "http://localhost:5080";

type RequestOptions = RequestInit & {
  skipAuth?: boolean;
  retry?: boolean;
};

export class ApiError extends Error {
  constructor(
    public readonly status: number,
    public readonly messages: string[],
    message = messages.join(" ")
  ) {
    super(message || `Request failed with status ${status}`);
  }
}

let onUnauthorized: (() => void) | null = null;
let refreshPromise: Promise<TokenResponse> | null = null;

export function setUnauthorizedHandler(handler: (() => void) | null): void {
  onUnauthorized = handler;
}

export async function apiRequest<T>(path: string, options: RequestOptions = {}): Promise<T> {
  const response = await fetch(`${API_BASE_URL}${path}`, buildRequestOptions(options));

  if (response.status === 401 && !options.skipAuth && options.retry !== false) {
    const refreshed = await tryRefreshToken();
    if (refreshed) {
      return apiRequest<T>(path, { ...options, retry: false });
    }
  }

  if (!response.ok) {
    throw await toApiError(response);
  }

  if (response.status === 204) {
    return undefined as T;
  }

  return (await response.json()) as T;
}

function buildRequestOptions(options: RequestOptions): RequestInit {
  const headers = new Headers(options.headers);

  if (options.body && !headers.has("Content-Type")) {
    headers.set("Content-Type", "application/json");
  }

  const tokens = getStoredTokens();
  if (!options.skipAuth && tokens?.accessToken) {
    headers.set("Authorization", `Bearer ${tokens.accessToken}`);
  }

  return {
    ...options,
    headers
  };
}

async function tryRefreshToken(): Promise<boolean> {
  const tokens = getStoredTokens();
  if (!tokens?.refreshToken) {
    handleUnauthorized();
    return false;
  }

  try {
    refreshPromise ??= apiRequest<TokenResponse>("/api/Auth/refresh", {
      method: "POST",
      skipAuth: true,
      retry: false,
      body: JSON.stringify({ refreshToken: tokens.refreshToken })
    });

    const refreshedTokens = await refreshPromise;
    setStoredTokens(refreshedTokens);
    return true;
  } catch {
    handleUnauthorized();
    return false;
  } finally {
    refreshPromise = null;
  }
}

async function toApiError(response: Response): Promise<ApiError> {
  let body: ApiErrorBody | null = null;

  try {
    body = (await response.json()) as ApiErrorBody;
  } catch {
    body = null;
  }

  const messages = body?.errors?.length
    ? body.errors
    : [body?.detail ?? body?.title ?? response.statusText];

  return new ApiError(response.status, messages);
}

function handleUnauthorized(): void {
  clearStoredTokens();
  onUnauthorized?.();
}
