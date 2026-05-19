import { useState } from "react";
import { ApiError } from "../services/api/httpClient";

export function useAsyncAction() {
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  async function run<T>(action: () => Promise<T>): Promise<T | null> {
    setIsLoading(true);
    setError(null);

    try {
      return await action();
    } catch (caught) {
      if (caught instanceof ApiError) {
        setError(caught.messages.join(" "));
      } else {
        setError("Ocurrió un error inesperado.");
      }

      return null;
    } finally {
      setIsLoading(false);
    }
  }

  return { isLoading, error, setError, run };
}
