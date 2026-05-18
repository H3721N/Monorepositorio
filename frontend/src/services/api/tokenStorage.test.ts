import { clearStoredTokens, getStoredTokens, setStoredTokens } from "./tokenStorage";
import type { TokenResponse } from "../../types/api";

const tokens: TokenResponse = {
  accessToken: "access-token",
  refreshToken: "refresh-token",
  accessTokenExpiresAt: "2026-05-18T12:00:00Z",
  refreshTokenExpiresAt: "2026-05-25T12:00:00Z"
};

describe("tokenStorage", () => {
  it("stores and returns token data", () => {
    setStoredTokens(tokens);

    expect(getStoredTokens()).toEqual(tokens);
  });

  it("clears token data", () => {
    setStoredTokens(tokens);

    clearStoredTokens();

    expect(getStoredTokens()).toBeNull();
  });

  it("removes invalid JSON from localStorage", () => {
    localStorage.setItem("monorepo.tokens", "{bad");

    expect(getStoredTokens()).toBeNull();
    expect(localStorage.getItem("monorepo.tokens")).toBeNull();
  });
});
