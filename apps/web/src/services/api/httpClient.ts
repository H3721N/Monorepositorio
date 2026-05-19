import axios, { AxiosError, type AxiosRequestConfig } from "axios";
import type { ApiErrorBody, TokenResponse } from "../../types/api";
import { clearStoredTokens, getStoredTokens, setStoredTokens } from "./tokenStorage";

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? "http://localhost:5080";

const apiClient = axios.create({
  baseURL: API_BASE_URL,
  adapter: "fetch"
});

type RequestOptions = AxiosRequestConfig & {
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
  try {
    const response = await apiClient.request<T>({
      url: path,
      ...buildRequestOptions(options)
    });

    if (response.status === 204) {
      return undefined as T;
    }

    return response.data;
  } catch (error) {
    if (axios.isAxiosError(error)) {
      if (error.response?.status === 401 && !options.skipAuth && options.retry !== false) {
        const refreshed = await tryRefreshToken();
        if (refreshed) {
          return apiRequest<T>(path, { ...options, retry: false });
        }
      }

      throw toApiError(error);
    }

    throw error;
  }
}

function buildRequestOptions(options: RequestOptions): AxiosRequestConfig {
  const { skipAuth, retry, ...axiosOptions } = options;
  const headers = { ...(options.headers as Record<string, string> | undefined) };

  if (options.data && !headers["Content-Type"]) {
    headers["Content-Type"] = "application/json";
  }

  const tokens = getStoredTokens();
  if (!options.skipAuth && tokens?.accessToken) {
    headers.Authorization = `Bearer ${tokens.accessToken}`;
  }

  return {
    ...axiosOptions,
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
    const refreshOptions: RequestOptions = {
      method: "POST",
      skipAuth: true,
      retry: false,
      data: { refreshToken: tokens.refreshToken }
    };

    refreshPromise ??= apiRequest<TokenResponse>("/api/Auth/refresh", refreshOptions);

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

function toApiError(error: AxiosError<ApiErrorBody>): ApiError {
  const body = error.response?.data ?? null;
  const status = error.response?.status ?? 0;

  const messages = body?.errors?.length
    ? body.errors
    : [body?.detail ?? body?.title ?? error.message];

  return new ApiError(status, messages);
}

function handleUnauthorized(): void {
  clearStoredTokens();
  onUnauthorized?.();
}
