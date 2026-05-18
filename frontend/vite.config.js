import { defineConfig } from "vitest/config";
import react from "@vitejs/plugin-react";
export default defineConfig({
    plugins: [react()],
    server: {
        port: 5173,
        strictPort: false
    },
    test: {
        environment: "jsdom",
        setupFiles: "./src/test/setup.ts",
        globals: true,
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
});
