import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, expect, it, vi } from "vitest";
import { AuthProvider, useAuth } from "./AuthContext";
import { jsonResponse } from "../test/http";

function Consumer() {
  const { logout } = useAuth();
  return <button onClick={() => void logout()}>Logout</button>;
}

describe("AuthContext", () => {
  it("clears session when logout receives unauthorized", async () => {
    localStorage.setItem(
      "monorepo.tokens",
      JSON.stringify({
        accessToken: "access",
        refreshToken: "refresh",
        accessTokenExpiresAt: "future",
        refreshTokenExpiresAt: "future"
      })
    );
    vi.stubGlobal(
      "fetch",
      vi
        .fn()
        .mockResolvedValueOnce(
          jsonResponse({ id: 1, email: "admin@ejemplo.com", roles: ["COUNTRY"], activo: true })
        )
        .mockResolvedValueOnce(jsonResponse({ errors: ["Unauthorized"] }, 401))
    );

    render(
      <AuthProvider>
        <Consumer />
      </AuthProvider>
    );

    await screen.findByRole("button", { name: "Logout" });
    await userEvent.click(screen.getByRole("button", { name: "Logout" }));

    await waitFor(() => expect(localStorage.getItem("monorepo.tokens")).toBeNull());
  });
});
