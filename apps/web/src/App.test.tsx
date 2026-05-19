import { render, screen, waitFor, within } from "@testing-library/react";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import userEvent from "@testing-library/user-event";
import { MemoryRouter } from "react-router-dom";
import { vi } from "vitest";
import { App } from "./App";
import { AuthProvider } from "./auth/AuthContext";
import type { Country, Department, RoleName, User } from "./types/api";
import { jsonResponse, emptyResponse } from "./test/http";

function renderApp() {
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } });
  return render(
    <QueryClientProvider client={queryClient}>
      <MemoryRouter initialEntries={["/login"]}>
        <AuthProvider>
          <App />
        </AuthProvider>
      </MemoryRouter>
    </QueryClientProvider>
  );
}

interface ApiMockOptions {
  loginSucceeds?: boolean;
  userRoles?: RoleName[];
}

function installApiMock(options: ApiMockOptions = {}) {
  let countries: Country[] = [{ id: 1, name: "Guatemala", isoCode: "GT" }];
  let departments: Department[] = [{ id: 1, name: "Guatemala", countryId: 1 }];
  let nextCountryId = 2;
  let nextDepartmentId = 2;
  let nextUserId = 10;
  const currentUser: User = {
    id: 1,
    email: "admin@ejemplo.com",
    roles: options.userRoles ?? ["COUNTRY", "DEPARTMENT", "USER_ADMIN"],
    activo: true
  };

  const fetchMock = vi.fn(async (input: RequestInfo | URL, init?: RequestInit) => {
    const request = input instanceof Request ? input : null;
    const url = new URL(request?.url ?? String(input));
    const method = init?.method ?? request?.method ?? "GET";
    const rawBody = init?.body ? String(init.body) : request ? await request.clone().text() : "";
    const body = rawBody ? JSON.parse(rawBody) : null;

    if (url.pathname === "/api/Auth/login" && method === "POST") {
      if (options.loginSucceeds === false) {
        return Promise.resolve(jsonResponse({ errors: ["Invalid credentials."] }, 401));
      }

      return Promise.resolve(
        jsonResponse({
          accessToken: "access-token",
          refreshToken: "refresh-token",
          accessTokenExpiresAt: "2026-05-18T12:15:00Z",
          refreshTokenExpiresAt: "2026-05-25T12:00:00Z"
        })
      );
    }

    if (url.pathname === "/api/Auth/logout" && method === "POST") {
      return Promise.resolve(emptyResponse());
    }

    if (url.pathname === "/api/Auth/change-password" && method === "POST") {
      return Promise.resolve(emptyResponse());
    }

    if (url.pathname === "/api/admin/users/me" && method === "GET") {
      return Promise.resolve(jsonResponse(currentUser));
    }

    if (url.pathname === "/api/admin/users" && method === "POST") {
      const created: User = {
        id: nextUserId++,
        email: body.email,
        roles: rolesFromIds(body.roleIds),
        activo: true
      };
      return Promise.resolve(jsonResponse(created, 201));
    }

    const roleMatch = url.pathname.match(/^\/api\/admin\/users\/(\d+)\/roles$/);
    if (roleMatch && method === "PUT") {
      return Promise.resolve(
        jsonResponse({
          id: Number(roleMatch[1]),
          email: "nuevo@ejemplo.com",
          roles: rolesFromIds(body.roleIds),
          activo: true
        })
      );
    }

    const deactivateMatch = url.pathname.match(/^\/api\/admin\/users\/(\d+)$/);
    if (deactivateMatch && method === "DELETE") {
      return Promise.resolve(emptyResponse());
    }

    if (url.pathname === "/api/Countries" && method === "GET") {
      return Promise.resolve(jsonResponse(countries));
    }

    if (url.pathname === "/api/Countries" && method === "POST") {
      if (countries.some((country) => country.name.toLowerCase() === String(body.name).toLowerCase())) {
        return Promise.resolve(jsonResponse({ errors: ["A country with the same name already exists."] }, 409));
      }

      const country = { id: nextCountryId++, name: body.name, isoCode: body.isoCode.toUpperCase() };
      countries = [...countries, country];
      return Promise.resolve(jsonResponse(country, 201));
    }

    const countryMatch = url.pathname.match(/^\/api\/Countries\/(\d+)$/);
    if (countryMatch && method === "PUT") {
      const id = Number(countryMatch[1]);
      if (countries.some((country) => country.id !== id && country.name.toLowerCase() === String(body.name).toLowerCase())) {
        return Promise.resolve(jsonResponse({ errors: ["A country with the same name already exists."] }, 409));
      }

      const updated = { id, name: body.name, isoCode: body.isoCode.toUpperCase() };
      countries = countries.map((country) => (country.id === id ? updated : country));
      return Promise.resolve(jsonResponse(updated));
    }

    if (countryMatch && method === "DELETE") {
      const id = Number(countryMatch[1]);
      countries = countries.filter((country) => country.id !== id);
      return Promise.resolve(emptyResponse());
    }

    if (url.pathname === "/api/Departments" && method === "GET") {
      const countryId = url.searchParams.get("countryId");
      const result = countryId
        ? departments.filter((department) => department.countryId === Number(countryId))
        : departments;
      return Promise.resolve(jsonResponse(result));
    }

    if (url.pathname === "/api/Departments" && method === "POST") {
      if (!countries.some((country) => country.id === Number(body.countryId))) {
        return Promise.resolve(jsonResponse({ errors: ["Country was not found."] }, 409));
      }

      const department = { id: nextDepartmentId++, name: body.name, countryId: Number(body.countryId) };
      departments = [...departments, department];
      return Promise.resolve(jsonResponse(department, 201));
    }

    const departmentMatch = url.pathname.match(/^\/api\/Departments\/(\d+)$/);
    if (departmentMatch && method === "PUT") {
      const id = Number(departmentMatch[1]);
      const updated = { id, name: body.name, countryId: Number(body.countryId) };
      departments = departments.map((department) => (department.id === id ? updated : department));
      return Promise.resolve(jsonResponse(updated));
    }

    if (departmentMatch && method === "DELETE") {
      const id = Number(departmentMatch[1]);
      departments = departments.filter((department) => department.id !== id);
      return Promise.resolve(emptyResponse());
    }

    return Promise.resolve(jsonResponse({ errors: [`Unhandled ${method} ${url.pathname}`] }, 500));
  });

  vi.stubGlobal("fetch", fetchMock);
  return fetchMock;
}

async function login() {
  await userEvent.click(screen.getByRole("button", { name: "Ingresar" }));
  await screen.findByRole("heading", { name: "Panel" });
}

describe("App integration and regression flows", () => {
  it("shows login when there is no stored session", () => {
    installApiMock();

    renderApp();

    expect(screen.getByRole("heading", { name: "Monorepo Admin" })).toBeInTheDocument();
    expect(screen.getByRole("button", { name: "Ingresar" })).toBeInTheDocument();
  });

  it("logs in and renders the administrative layout with role-based navigation", async () => {
    const fetchMock = installApiMock();
    renderApp();

    await login();

    expect(screen.getByText("admin@ejemplo.com")).toBeInTheDocument();
    expect(screen.getByRole("link", { name: "Países" })).toBeInTheDocument();
    expect(screen.getByRole("link", { name: "Departamentos" })).toBeInTheDocument();
    expect(screen.getByRole("link", { name: "Usuarios" })).toBeInTheDocument();
    const calledUrls = fetchMock.mock.calls.map(([input]) => (input instanceof Request ? input.url : String(input)));
    expect(calledUrls).toContain("http://localhost:5080/api/Auth/login");
    expect(calledUrls).toContain("http://localhost:5080/api/admin/users/me");
  });

  it("shows backend error when login fails", async () => {
    installApiMock({ loginSucceeds: false });
    renderApp();

    await userEvent.click(screen.getByRole("button", { name: "Ingresar" }));

    expect(await screen.findByText("Invalid credentials.")).toBeInTheDocument();
    expect(screen.queryByRole("heading", { name: "Panel" })).not.toBeInTheDocument();
  });

  it("hides unauthorized navigation items for users without admin roles", async () => {
    installApiMock({ userRoles: ["COUNTRY"] });
    renderApp();

    await login();

    expect(screen.getByRole("link", { name: "Países" })).toBeInTheDocument();
    expect(screen.queryByRole("link", { name: "Departamentos" })).not.toBeInTheDocument();
    expect(screen.queryByRole("link", { name: "Usuarios" })).not.toBeInTheDocument();
  });

  it("creates, updates, rejects duplicate and deletes countries", async () => {
    installApiMock();
    renderApp();
    await login();
    await userEvent.click(screen.getByRole("link", { name: "Países" }));
    await screen.findByRole("heading", { name: "Nuevo país" });

    await userEvent.type(screen.getByLabelText("Nombre"), "El Salvador");
    await userEvent.type(screen.getByLabelText("ISO"), "SV");
    await userEvent.click(screen.getByRole("button", { name: "Crear" }));

    expect(await screen.findByText("País creado.")).toBeInTheDocument();
    expect(await screen.findByText("El Salvador")).toBeInTheDocument();

    const elSalvadorRow = screen.getByRole("row", { name: /El Salvador/ });
    await userEvent.click(within(elSalvadorRow).getByRole("button", { name: "Editar" }));
    await userEvent.clear(screen.getByLabelText("Nombre"));
    await userEvent.type(screen.getByLabelText("Nombre"), "Guatemala");
    await userEvent.click(screen.getByRole("button", { name: "Actualizar" }));

    expect(await screen.findByText("A country with the same name already exists.")).toBeInTheDocument();

    vi.spyOn(window, "confirm").mockReturnValue(true);
    await userEvent.click(within(elSalvadorRow).getByRole("button", { name: "Eliminar" }));

    expect(await screen.findByText("País eliminado.")).toBeInTheDocument();
  });

  it("creates, filters, updates, rejects invalid country and deletes departments", async () => {
    installApiMock();
    renderApp();
    await login();
    await userEvent.click(screen.getByRole("link", { name: "Departamentos" }));
    await screen.findByRole("heading", { name: "Nuevo departamento" });

    await userEvent.type(screen.getByLabelText("Nombre"), "Quetzaltenango");
    await userEvent.selectOptions(screen.getByLabelText("País"), "1");
    await userEvent.click(screen.getByRole("button", { name: "Crear" }));

    expect(await screen.findByText("Departamento creado.")).toBeInTheDocument();
    expect(await screen.findByText("Quetzaltenango")).toBeInTheDocument();

    const filter = screen.getByDisplayValue("Todos los países");
    await userEvent.selectOptions(filter, "1");
    expect(await screen.findByText("Quetzaltenango")).toBeInTheDocument();

    const row = screen.getByRole("row", { name: /Quetzaltenango/ });
    await userEvent.click(within(row).getByRole("button", { name: "Editar" }));
    await userEvent.clear(screen.getByLabelText("Nombre"));
    await userEvent.type(screen.getByLabelText("Nombre"), "Petén");
    await userEvent.click(screen.getByRole("button", { name: "Actualizar" }));
    expect(await screen.findByText("Departamento actualizado.")).toBeInTheDocument();

    vi.spyOn(window, "confirm").mockReturnValue(true);
    const updatedRow = await screen.findByRole("row", { name: /Petén/ });
    await userEvent.click(within(updatedRow).getByRole("button", { name: "Eliminar" }));
    expect(await screen.findByText("Departamento eliminado.")).toBeInTheDocument();
  });

  it("creates users, changes roles and deactivates users", async () => {
    installApiMock();
    renderApp();
    await login();
    await userEvent.click(screen.getByRole("link", { name: "Usuarios" }));

    await userEvent.type(screen.getByLabelText("Email"), "nuevo@ejemplo.com");
    await userEvent.type(screen.getByLabelText("Contraseña"), "User123!");
    await userEvent.click(screen.getAllByRole("checkbox", { name: "Países ID 1" })[0]);
    await userEvent.click(screen.getByRole("button", { name: "Crear usuario" }));

    expect(await screen.findByText("Usuario creado con ID 10.")).toBeInTheDocument();
    expect(await screen.findByText("nuevo@ejemplo.com")).toBeInTheDocument();

    await userEvent.type(screen.getByLabelText("ID del usuario"), "10");
    await userEvent.click(screen.getAllByRole("checkbox", { name: "Usuarios ID 3" })[1]);
    await userEvent.click(screen.getByRole("button", { name: "Actualizar roles" }));
    expect(await screen.findByText("Roles actualizados para el usuario 10.")).toBeInTheDocument();

    await userEvent.click(screen.getByRole("button", { name: "Usar ID" }));
    expect(screen.getByLabelText("ID del usuario")).toHaveValue(10);

    vi.spyOn(window, "confirm").mockReturnValue(true);
    await userEvent.click(screen.getByRole("button", { name: "Desactivar" }));
    expect(await screen.findByText("Usuario 10 desactivado.")).toBeInTheDocument();
  });

  it("changes password and logs out", async () => {
    installApiMock();
    renderApp();
    await login();
    await userEvent.click(screen.getByRole("link", { name: "Cuenta" }));

    await userEvent.type(screen.getByLabelText("Contraseña actual"), "Admin123!");
    await userEvent.type(screen.getByLabelText("Nueva contraseña"), "NewAdmin123!");
    await userEvent.click(screen.getByRole("button", { name: "Actualizar contraseña" }));
    expect(await screen.findByText("Contraseña actualizada. Las sesiones previas quedaron invalidadas.")).toBeInTheDocument();

    await userEvent.click(screen.getByRole("button", { name: "Cerrar sesión" }));
    await waitFor(() => expect(screen.getByRole("heading", { name: "Monorepo Admin" })).toBeInTheDocument());
  });

  it("loads current user from stored token and clears session if /me fails", async () => {
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
      vi.fn(() => Promise.resolve(jsonResponse({ errors: ["Unauthorized"] }, 401)))
    );

    renderApp();

    expect(await screen.findByRole("heading", { name: "Monorepo Admin" })).toBeInTheDocument();
    expect(localStorage.getItem("monorepo.tokens")).toBeNull();
  });
});

function rolesFromIds(ids: number[]): RoleName[] {
  return ids.map((id) => {
    if (id === 1) {
      return "COUNTRY";
    }
    if (id === 2) {
      return "DEPARTMENT";
    }
    return "USER_ADMIN";
  });
}
