import { defineConfig, type UserConfig } from "vite";
import type { InlineConfig } from "vitest";
import { configDefaults } from "vitest/config";
import react from "@vitejs/plugin-react";
import tailwindcss from "@tailwindcss/vite";

export default defineConfig({
  plugins: [react(), tailwindcss()],
  server: {
    port: 5173,
    strictPort: false
  },
  test: {
    environment: "jsdom",
    setupFiles: "./src/test/setup.ts",
    globals: true,
    exclude: [...configDefaults.exclude, "e2e/**", "playwright-report/**"],
    coverage: {
      provider: "v8",
      reporter: ["text", "html", "json-summary"],
      include: ["src/**/*.{ts,tsx}"],
      exclude: [
        "src/main.tsx",
        "src/vite-env.d.ts",
        "src/**/*.test.{ts,tsx}",
        "src/test/**",
        "src/types/**",
        "src/routes/views.ts"
      ],
      thresholds: {
        statements: 90,
        lines: 90,
        branches: 85,
        functions: 90
      }
    }
  }
} as UserConfig & { test: InlineConfig });
