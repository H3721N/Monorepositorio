import { FormEvent, useState } from "react";
import { useAuth } from "../auth/AuthContext";
import { StatusMessage } from "../components/StatusMessage";
import { useAsyncAction } from "../hooks/useAsyncAction";
import { authApi } from "../services/api/endpoints";

export function AccountPage() {
  const { user } = useAuth();
  const { isLoading, error, run } = useAsyncAction();
  const [currentPassword, setCurrentPassword] = useState("");
  const [newPassword, setNewPassword] = useState("");
  const [success, setSuccess] = useState<string | null>(null);

  async function handleSubmit(event: FormEvent) {
    event.preventDefault();
    setSuccess(null);

    const result = await run(() => authApi.changePassword(currentPassword, newPassword));
    if (result === undefined) {
      setCurrentPassword("");
      setNewPassword("");
      setSuccess("Contraseña actualizada. Las sesiones previas quedaron invalidadas.");
    }
  }

  return (
    <section className="page-section">
      <div className="page-heading">
        <div>
          <h1>Cuenta</h1>
          <p>{user?.email}</p>
        </div>
      </div>

      <section className="surface narrow-surface">
        <h2>Cambiar contraseña</h2>
        <form className="form-stack" onSubmit={handleSubmit}>
          <label>
            Contraseña actual
            <input
              value={currentPassword}
              onChange={(event) => setCurrentPassword(event.target.value)}
              type="password"
              required
            />
          </label>
          <label>
            Nueva contraseña
            <input
              value={newPassword}
              minLength={8}
              onChange={(event) => setNewPassword(event.target.value)}
              type="password"
              required
            />
          </label>
          {error && <StatusMessage type="error">{error}</StatusMessage>}
          {success && <StatusMessage type="success">{success}</StatusMessage>}
          <button className="primary-button" disabled={isLoading}>
            {isLoading ? "Actualizando..." : "Actualizar contraseña"}
          </button>
        </form>
      </section>
    </section>
  );
}
