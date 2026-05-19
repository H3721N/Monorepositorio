import { departmentsApi } from "./endpoints";
import { ApiError } from "./httpClient";
import { jsonResponse, mockFetch } from "../../test/http";

describe("api endpoints", () => {
  it("surfaces department relation errors returned by the backend", async () => {
    mockFetch(jsonResponse({ errors: ["Country was not found."] }, 409));

    await expect(departmentsApi.create({ name: "Missing", countryId: 999 })).rejects.toMatchObject<ApiError>({
      status: 409,
      messages: ["Country was not found."]
    });
  });
});
