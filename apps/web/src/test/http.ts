import { vi } from "vitest";

export function jsonResponse(body: unknown, status = 200): Response {
  return new Response(JSON.stringify(body), {
    status,
    headers: { "Content-Type": "application/json" }
  });
}

export function emptyResponse(status = 204): Response {
  return new Response(null, { status });
}

export function mockFetch(...responses: Array<Response | ((input: RequestInfo | URL, init?: RequestInit) => Response)>): void {
  const fetchMock = vi.fn();

  for (const response of responses) {
    fetchMock.mockImplementationOnce((input: RequestInfo | URL, init?: RequestInit) =>
      Promise.resolve(typeof response === "function" ? response(input, init) : response)
    );
  }

  vi.stubGlobal("fetch", fetchMock);
}

export function fetchMock(): ReturnType<typeof vi.fn> {
  return fetch as unknown as ReturnType<typeof vi.fn>;
}
