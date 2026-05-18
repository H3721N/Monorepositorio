import { FormEvent, useState } from "react";
import { ConfirmButton } from "../components/ConfirmButton";
import { StatusMessage } from "../components/StatusMessage";
import { useAsyncAction } from "../hooks/useAsyncAction";
import { userApi } from "../services/api/endpoints";
import type { User } from "../types/api";
import { ROLE_OPTIONS, roleLabel } from "../utils/roles";

export function UsersPage() {
  const { isLoading, error, run } = useAsyncAction();
  const [createdUsers, setCreatedUsers] = useState<User[]>([]);
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [roleIds, setRoleIds] = useState<number[]>([]);
  const [targetUserId, setTargetUserId] = useState("");
  const [targetRoleIds, setTargetRoleIds] = useState<number[]>([]);
  const [success, setSuccess] = useState<string | null>(null);

  async function handleCreate(event: FormEvent) {
    event.preventDefault();
    setSuccess(null);

    const created = await run(() => userApi.create({ email, password, roleIds }));
    if (created) {
      setCreatedUsers((current) => [created, ...current.filter((user) => user.id !== created.id)]);
      setEmail("");
      setPassword("");
      setRoleIds([]);
      setSuccess(`Usuario creado con ID ${created.id}.`);
    }
  }

  async function handleUpdateRoles(event: FormEvent) {
    event.preventDefault();
    setSuccess(null);

    const updated = await run(() => userApi.updateRoles(Number(targetUserId), { roleIds: targetRoleIds }));
    if (updated) {
      setCreatedUsers((current) => [updated, ...current.filter((user) => user.id !== updated.id)]);
      setSuccess(`Roles actualizados para el usuario ${updated.id}.`);
    }
  }

  async function handleDeactivate(id: number) {
    setSuccess(null);
    const result = await run(() => userApi.deactivate(id));
    if (result === undefined) {
      setCreatedUsers((current) =>
        current.map((user) => (user.id === id ? { ...user, activo: false } : user))
      );
      setSuccess(`Usuario ${id} desactivado.`);
    }
  }

  return (
    <section className="page-section">
      <div className="page-heading">
        <div>
          <h1>Usuarios</h1>
          <p>Administración protegida por el rol USER_ADMIN.</p>
        </div>
      </div>

      <div className="split-layout">
        <section className="surface">
          <h2>Crear usuario</h2>
          <form className="form-stack" onSubmit={handleCreate}>
            <label>
              Email
              <input value={email} onChange={(event) => setEmail(event.target.value)} type="email" required />
            </label>
            <label>
              Contraseña
              <input value={password} onChange={(event) => setPassword(event.target.value)} type="password" required />
            </label>
            <RoleChecklist selected={roleIds} onChange={setRoleIds} />

            <button className="primary-button" disabled={isLoading || roleIds.length === 0}>
              {isLoading ? "Creando..." : "Crear usuario"}
            </button>
          </form>
        </section>

        <section className="surface">
          <h2>Cambiar roles</h2>
          <form className="form-stack" onSubmit={handleUpdateRoles}>
            <label>
              ID del usuario
              <input
                value={targetUserId}
                min={1}
                onChange={(event) => setTargetUserId(event.target.value)}
                type="number"
                required
              />
            </label>
            <RoleChecklist selected={targetRoleIds} onChange={setTargetRoleIds} />
            <button className="primary-button" disabled={isLoading || targetRoleIds.length === 0}>
              Actualizar roles
            </button>
          </form>
        </section>
      </div>

      {(error || success) && (
        <div className="message-block">
          {error && <StatusMessage type="error">{error}</StatusMessage>}
          {success && <StatusMessage type="success">{success}</StatusMessage>}
        </div>
      )}

      <section className="surface table-surface">
        <div className="surface-header">
          <h2>Usuarios creados en esta sesión</h2>
          <span className="muted-text">La API actual no expone listado global de usuarios.</span>
        </div>
        <div className="table-wrap">
          <table>
            <thead>
              <tr>
                <th>ID</th>
                <th>Email</th>
                <th>Roles</th>
                <th>Estado</th>
                <th>Acciones</th>
              </tr>
            </thead>
            <tbody>
              {createdUsers.map((user) => (
                <tr key={user.id}>
                  <td>{user.id}</td>
                  <td>{user.email}</td>
                  <td>{user.roles.map(roleLabel).join(", ")}</td>
                  <td>{user.activo ? "Activo" : "Inactivo"}</td>
                  <td className="action-cell">
                    <button
                      className="secondary-button"
                      onClick={() => {
                        setTargetUserId(String(user.id));
                        setTargetRoleIds(user.roles.map((role) => ROLE_OPTIONS.find((option) => option.name === role)!.id));
                      }}
                    >
                      Usar ID
                    </button>
                    <ConfirmButton
                      className="danger-button"
                      disabled={!user.activo}
                      message={`¿Desactivar ${user.email}?`}
                      onConfirm={() => handleDeactivate(user.id)}
                    >
                      Desactivar
                    </ConfirmButton>
                  </td>
                </tr>
              ))}
              {createdUsers.length === 0 && (
                <tr>
                  <td colSpan={5} className="empty-state">
                    Crea un usuario para verlo aquí.
                  </td>
                </tr>
              )}
            </tbody>
          </table>
        </div>
      </section>
    </section>
  );
}

function RoleChecklist({ selected, onChange }: { selected: number[]; onChange: (ids: number[]) => void }) {
  function toggle(roleId: number) {
    onChange(selected.includes(roleId) ? selected.filter((id) => id !== roleId) : [...selected, roleId]);
  }

  return (
    <fieldset className="role-checklist">
      <legend>Roles</legend>
      {ROLE_OPTIONS.map((role) => (
        <label key={role.id} className="check-row">
          <input checked={selected.includes(role.id)} type="checkbox" onChange={() => toggle(role.id)} />
          <span>{role.label}</span>
          <small>ID {role.id}</small>
        </label>
      ))}
    </fieldset>
  );
}
