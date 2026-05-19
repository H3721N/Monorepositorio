import { FormEvent, useState } from "react";
import { z } from "zod";
import { useAuth } from "../auth/AuthContext";
import { StatusMessage } from "../components/StatusMessage";
import { useAsyncAction } from "../hooks/useAsyncAction";

const loginSchema = z.object({
  email: z.string().email("Ingresa un email válido."),
  password: z.string().min(1, "Ingresa la contraseña.")
});

export function LoginPage() {
  const { login } = useAuth();
  const { isLoading, error, run } = useAsyncAction();
  const [email, setEmail] = useState("admin@ejemplo.com");
  const [password, setPassword] = useState("Admin123!");
  const [validationError, setValidationError] = useState<string | null>(null);

  async function handleSubmit(event: FormEvent) {
    event.preventDefault();
    const parsed = loginSchema.safeParse({ email, password });
    if (!parsed.success) {
      setValidationError(parsed.error.issues[0]?.message ?? "Datos inválidos.");
      return;
    }

    setValidationError(null);
    await run(() => login(parsed.data.email, parsed.data.password));
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

          {validationError && <StatusMessage type="error">{validationError}</StatusMessage>}
          {error && <StatusMessage type="error">{error}</StatusMessage>}

          <button className="primary-button" disabled={isLoading}>
            {isLoading ? "Ingresando..." : "Ingresar"}
          </button>
        </form>
      </section>
    </main>
  );
}
