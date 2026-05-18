import { FormEvent, useState } from "react";
import { useAuth } from "../auth/AuthContext";
import { StatusMessage } from "../components/StatusMessage";
import { useAsyncAction } from "../hooks/useAsyncAction";

export function LoginPage() {
  const { login } = useAuth();
  const { isLoading, error, run } = useAsyncAction();
  const [email, setEmail] = useState("admin@ejemplo.com");
  const [password, setPassword] = useState("Admin123!");

  async function handleSubmit(event: FormEvent) {
    event.preventDefault();
    await run(() => login(email, password));
  }

  return (
    <main className="login-page">
      <section className="login-panel">
        <div className="login-heading">
          <span className="brand-mark">M</span>
          <div>
            <h1>Monorepo Admin</h1>
            <p>Acceso administrativo</p>
          </div>
        </div>

        <form className="form-stack" onSubmit={handleSubmit}>
          <label>
            Email
            <input value={email} onChange={(event) => setEmail(event.target.value)} type="email" required />
          </label>
          <label>
            Contraseña
            <input value={password} onChange={(event) => setPassword(event.target.value)} type="password" required />
          </label>

          {error && <StatusMessage type="error">{error}</StatusMessage>}

          <button className="primary-button" disabled={isLoading}>
            {isLoading ? "Ingresando..." : "Ingresar"}
          </button>
        </form>
      </section>
    </main>
  );
}
