import { FormEvent, useEffect, useMemo, useState } from "react";
import { ConfirmButton } from "../components/ConfirmButton";
import { StatusMessage } from "../components/StatusMessage";
import { useAsyncAction } from "../hooks/useAsyncAction";
import { countriesApi, departmentsApi } from "../services/api/endpoints";
import type { Country, Department } from "../types/api";

interface DepartmentFormState {
  id?: number;
  name: string;
  countryId: string;
}

const emptyForm: DepartmentFormState = { name: "", countryId: "" };

export function DepartmentsPage() {
  const { isLoading, error, run } = useAsyncAction();
  const [countries, setCountries] = useState<Country[]>([]);
  const [departments, setDepartments] = useState<Department[]>([]);
  const [selectedCountryId, setSelectedCountryId] = useState("");
  const [form, setForm] = useState<DepartmentFormState>(emptyForm);
  const [success, setSuccess] = useState<string | null>(null);
  const isEditing = useMemo(() => typeof form.id === "number", [form.id]);

  async function loadCountries() {
    const data = await run(() => countriesApi.list());
    if (data) {
      setCountries(data);
    }
  }

  async function loadDepartments(countryId = selectedCountryId) {
    const parsedCountryId = countryId ? Number(countryId) : undefined;
    const data = await run(() => departmentsApi.list(parsedCountryId));
    if (data) {
      setDepartments(data);
    }
  }

  useEffect(() => {
    void loadCountries();
    void loadDepartments("");
  }, []);

  async function handleSubmit(event: FormEvent) {
    event.preventDefault();
    setSuccess(null);

    const payload = { name: form.name.trim(), countryId: Number(form.countryId) };
    const saved = await run(() =>
      isEditing ? departmentsApi.update(form.id!, payload) : departmentsApi.create(payload)
    );

    if (saved) {
      setForm(emptyForm);
      setSuccess(isEditing ? "Departamento actualizado." : "Departamento creado.");
      await loadDepartments();
    }
  }

  async function handleDelete(department: Department) {
    setSuccess(null);
    const removed = await run(() => departmentsApi.remove(department.id));
    if (removed === undefined) {
      setSuccess("Departamento eliminado.");
      await loadDepartments();
    }
  }

  return (
    <section className="page-section">
      <div className="page-heading">
        <div>
          <h1>Departamentos</h1>
          <p>Catálogo protegido por el rol DEPARTMENT.</p>
        </div>
      </div>

      <div className="split-layout">
        <section className="surface">
          <h2>{isEditing ? "Editar departamento" : "Nuevo departamento"}</h2>
          <form className="form-stack" onSubmit={handleSubmit}>
            <label>
              Nombre
              <input
                value={form.name}
                maxLength={100}
                onChange={(event) => setForm((current) => ({ ...current, name: event.target.value }))}
                required
              />
            </label>
            <label>
              País
              <select
                value={form.countryId}
                onChange={(event) => setForm((current) => ({ ...current, countryId: event.target.value }))}
                required
              >
                <option value="">Seleccionar</option>
                {countries.map((country) => (
                  <option key={country.id} value={country.id}>
                    {country.name}
                  </option>
                ))}
              </select>
            </label>
            {error && <StatusMessage type="error">{error}</StatusMessage>}
            {success && <StatusMessage type="success">{success}</StatusMessage>}
            <div className="button-row">
              <button className="primary-button" disabled={isLoading}>
                {isLoading ? "Guardando..." : isEditing ? "Actualizar" : "Crear"}
              </button>
              {isEditing && (
                <button type="button" className="secondary-button" onClick={() => setForm(emptyForm)}>
                  Cancelar
                </button>
              )}
            </div>
          </form>
        </section>

        <section className="surface table-surface">
          <div className="surface-header">
            <h2>Listado</h2>
            <div className="inline-controls">
              <select
                value={selectedCountryId}
                onChange={(event) => {
                  setSelectedCountryId(event.target.value);
                  void loadDepartments(event.target.value);
                }}
              >
                <option value="">Todos los países</option>
                {countries.map((country) => (
                  <option key={country.id} value={country.id}>
                    {country.name}
                  </option>
                ))}
              </select>
              <button className="secondary-button" onClick={() => loadDepartments()} disabled={isLoading}>
                Recargar
              </button>
            </div>
          </div>

          <div className="table-wrap">
            <table>
              <thead>
                <tr>
                  <th>ID</th>
                  <th>Nombre</th>
                  <th>País</th>
                  <th>Acciones</th>
                </tr>
              </thead>
              <tbody>
                {departments.map((department) => (
                  <tr key={department.id}>
                    <td>{department.id}</td>
                    <td>{department.name}</td>
                    <td>{countries.find((country) => country.id === department.countryId)?.name ?? department.countryId}</td>
                    <td className="action-cell">
                      <button
                        className="secondary-button"
                        onClick={() => setForm({ id: department.id, name: department.name, countryId: String(department.countryId) })}
                      >
                        Editar
                      </button>
                      <ConfirmButton
                        className="danger-button"
                        message={`¿Eliminar ${department.name}?`}
                        onConfirm={() => handleDelete(department)}
                      >
                        Eliminar
                      </ConfirmButton>
                    </td>
                  </tr>
                ))}
                {departments.length === 0 && (
                  <tr>
                    <td colSpan={4} className="empty-state">
                      No hay departamentos registrados.
                    </td>
                  </tr>
                )}
              </tbody>
            </table>
          </div>
        </section>
      </div>
    </section>
  );
}
