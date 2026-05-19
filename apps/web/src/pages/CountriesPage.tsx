import { FormEvent, useEffect, useMemo, useState } from "react";
import { ConfirmButton } from "../components/ConfirmButton";
import { StatusMessage } from "../components/StatusMessage";
import { useAsyncAction } from "../hooks/useAsyncAction";
import { countriesApi } from "../services/api/endpoints";
import type { Country } from "../types/api";

interface CountryFormState {
  id?: number;
  name: string;
  isoCode: string;
}

const emptyForm: CountryFormState = { name: "", isoCode: "" };

export function CountriesPage() {
  const { isLoading, error, run } = useAsyncAction();
  const [countries, setCountries] = useState<Country[]>([]);
  const [form, setForm] = useState<CountryFormState>(emptyForm);
  const [success, setSuccess] = useState<string | null>(null);
  const isEditing = useMemo(() => typeof form.id === "number", [form.id]);

  async function loadCountries() {
    const data = await run(() => countriesApi.list());
    if (data) {
      setCountries(data);
    }
  }

  useEffect(() => {
    void loadCountries();
  }, []);

  async function handleSubmit(event: FormEvent) {
    event.preventDefault();
    setSuccess(null);

    const payload = { name: form.name.trim(), isoCode: form.isoCode.trim() };
    const saved = await run(() => (isEditing ? countriesApi.update(form.id!, payload) : countriesApi.create(payload)));

    if (saved) {
      setForm(emptyForm);
      setSuccess(isEditing ? "País actualizado." : "País creado.");
      await loadCountries();
    }
  }

  async function handleDelete(country: Country) {
    setSuccess(null);
    const removed = await run(() => countriesApi.remove(country.id));
    if (removed === undefined) {
      setSuccess("País eliminado.");
      await loadCountries();
    }
  }

  return (
    <section className="page-section">
      <div className="page-heading">
        <div>
          <h1>Países</h1>
          <p>Catálogo protegido por el rol COUNTRY.</p>
        </div>
      </div>

      <div className="split-layout">
        <section className="surface">
          <h2>{isEditing ? "Editar país" : "Nuevo país"}</h2>
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
              ISO
              <input
                value={form.isoCode}
                maxLength={2}
                onChange={(event) => setForm((current) => ({ ...current, isoCode: event.target.value.toUpperCase() }))}
                required
              />
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
            <button className="secondary-button" onClick={loadCountries} disabled={isLoading}>
              Recargar
            </button>
          </div>

          <div className="table-wrap">
            <table>
              <thead>
                <tr>
                  <th>ID</th>
                  <th>Nombre</th>
                  <th>ISO</th>
                  <th>Acciones</th>
                </tr>
              </thead>
              <tbody>
                {countries.map((country) => (
                  <tr key={country.id}>
                    <td>{country.id}</td>
                    <td>{country.name}</td>
                    <td>{country.isoCode}</td>
                    <td className="action-cell">
                      <button className="secondary-button" onClick={() => setForm(country)}>
                        Editar
                      </button>
                      <ConfirmButton
                        className="danger-button"
                        message={`¿Eliminar ${country.name}?`}
                        onConfirm={() => handleDelete(country)}
                      >
                        Eliminar
                      </ConfirmButton>
                    </td>
                  </tr>
                ))}
                {countries.length === 0 && (
                  <tr>
                    <td colSpan={4} className="empty-state">
                      No hay países registrados.
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
