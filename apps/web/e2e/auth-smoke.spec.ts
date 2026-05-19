import { expect, test } from "@playwright/test";

test("login flow renders the protected dashboard", async ({ page }) => {
  await page.route("http://localhost:5080/api/Auth/login", async (route) => {
    await route.fulfill({
      status: 200,
      contentType: "application/json",
      body: JSON.stringify({
        accessToken: "access-token",
        refreshToken: "refresh-token"
      })
    });
  });

  await page.route("http://localhost:5080/api/admin/users/me", async (route) => {
    await route.fulfill({
      status: 200,
      contentType: "application/json",
      body: JSON.stringify({
        id: 1,
        email: "admin@ejemplo.com",
        roles: ["COUNTRY", "DEPARTMENT", "USER_ADMIN"]
      })
    });
  });

  await page.goto("/login");

  await page.getByLabel("Email").fill("admin@ejemplo.com");
  await page.getByLabel("Contraseña").fill("Admin123!");
  await page.getByRole("button", { name: "Ingresar" }).click();

  await expect(page.getByRole("heading", { name: "Panel" })).toBeVisible();
  await expect(page.getByRole("link", { name: "Usuarios" })).toBeVisible();
});
