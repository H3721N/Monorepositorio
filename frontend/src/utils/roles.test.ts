import { roleLabel } from "./roles";
import type { RoleName } from "../types/api";

describe("roles", () => {
  it("returns friendly labels for known roles", () => {
    expect(roleLabel("COUNTRY")).toBe("Países");
  });

  it("falls back to the role value for unknown values", () => {
    expect(roleLabel("UNKNOWN" as RoleName)).toBe("UNKNOWN");
  });
});
