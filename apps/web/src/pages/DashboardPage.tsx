import { useAuth } from "../auth/AuthContext";
import { roleLabel } from "../utils/roles";

export function DashboardPage() {
  const { user, hasRole } = useAuth();

  const cards = [
    { label: "Países", value: hasRole("COUNTRY") ? "Disponible" : "Sin acceso" },
    { label: "Departamentos", value: hasRole("DEPARTMENT") ? "Disponible" : "Sin acceso" },
    { label: "Usuarios", value: hasRole("USER_ADMIN") ? "Disponible" : "Sin acceso" }
  ];

  return (
    <section className="page-section">
      <div className="page-heading">
        <div>
          <h1>Panel</h1>
          <p>Sesión activa como {user?.email}</p>
        </div>
      </div>

      <div className="metric-grid">
        {cards.map((card) => (
          <article className="metric" key={card.label}>
            <span>{card.label}</span>
            <strong>{card.value}</strong>
          </article>
        ))}
      </div>

      <section className="surface">
        <h2>Permisos actuales</h2>
        <div className="pill-row">
          {user?.roles.map((role) => (
            <span className="pill" key={role}>
              {roleLabel(role)}
            </span>
          ))}
        </div>
      </section>
    </section>
  );
}
