import { apiRequest, ApiError, setUnauthorizedHandler } from "./httpClient";
import { getStoredTokens, setStoredTokens } from "./tokenStorage";
import { emptyResponse, fetchMock, jsonResponse, mockFetch } from "../../test/http";

describe("httpClient", () => {
  it("adds bearer token to protected requests", async () => {
    setStoredTokens({
      accessToken: "access",
      refreshToken: "refresh",
      accessTokenExpiresAt: "soon",
      refreshTokenExpiresAt: "later"
    });
    mockFetch(jsonResponse({ ok: true }));

    await apiRequest<{ ok: boolean }>("/api/Countries");

    expect(fetchMock()).toHaveBeenCalledWith(
      "http://localhost:5080/api/Countries",
      expect.objectContaining({
        headers: expect.any(Headers)
      })
    );
    const headers = fetchMock().mock.calls[0][1].headers as Headers;
    expect(headers.get("Authorization")).toBe("Bearer access");
  });

  it("throws ApiError with backend messages", async () => {
    mockFetch(jsonResponse({ errors: ["A country with the same name already exists."] }, 409));

    await expect(apiRequest("/api/Countries")).rejects.toMatchObject<ApiError>({
      status: 409,
      messages: ["A country with the same name already exists."]
    });
  });

  it("refreshes token and retries original request after unauthorized response", async () => {
    setStoredTokens({
      accessToken: "expired",
      refreshToken: "refresh",
      accessTokenExpiresAt: "past",
      refreshTokenExpiresAt: "future"
    });
    mockFetch(
      jsonResponse({ errors: ["Unauthorized"] }, 401),
      jsonResponse({
        accessToken: "new-access",
        refreshToken: "new-refresh",
        accessTokenExpiresAt: "future",
        refreshTokenExpiresAt: "future"
      }),
      jsonResponse([{ id: 1, name: "Guatemala", isoCode: "GT" }])
    );

    const countries = await apiRequest<Array<{ id: number }>>("/api/Countries");

    expect(countries).toEqual([{ id: 1, name: "Guatemala", isoCode: "GT" }]);
    expect(getStoredTokens()?.accessToken).toBe("new-access");
    expect(fetchMock()).toHaveBeenCalledTimes(3);
  });

  it("clears session and calls unauthorized handler when refresh fails", async () => {
    const handler = vi.fn();
    setUnauthorizedHandler(handler);
    setStoredTokens({
      accessToken: "expired",
      refreshToken: "refresh",
      accessTokenExpiresAt: "past",
      refreshTokenExpiresAt: "future"
    });
    mockFetch(jsonResponse({ errors: ["Unauthorized"] }, 401), jsonResponse({ errors: ["Invalid refresh token."] }, 401));

    await expect(apiRequest("/api/Countries")).rejects.toBeInstanceOf(ApiError);

    expect(getStoredTokens()).toBeNull();
    expect(handler).toHaveBeenCalled();
    setUnauthorizedHandler(null);
  });

  it("clears session immediately when unauthorized happens without a refresh token", async () => {
    const handler = vi.fn();
    setUnauthorizedHandler(handler);
    mockFetch(jsonResponse({ errors: ["Unauthorized"] }, 401));

    await expect(apiRequest("/api/Countries")).rejects.toBeInstanceOf(ApiError);

    expect(handler).toHaveBeenCalled();
    setUnauthorizedHandler(null);
  });

  it("uses status text when an error response has no JSON body", async () => {
    vi.stubGlobal("fetch", vi.fn(() => Promise.resolve(new Response("not-json", { status: 500, statusText: "Server Error" }))));

    await expect(apiRequest("/api/Countries")).rejects.toMatchObject<ApiError>({
      status: 500,
      messages: ["Server Error"]
    });
  });

  it("returns undefined for no-content responses", async () => {
    mockFetch(emptyResponse());

    await expect(apiRequest<void>("/api/Auth/logout", { method: "POST" })).resolves.toBeUndefined();
  });
});
