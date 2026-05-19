import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { useAsyncAction } from "./useAsyncAction";

function Harness() {
  const { error, run } = useAsyncAction();
  return (
    <>
      <button onClick={() => void run(async () => Promise.reject(new Error("boom")))}>Run</button>
      {error && <p>{error}</p>}
    </>
  );
}

describe("useAsyncAction", () => {
  it("maps unexpected errors to a generic message", async () => {
    render(<Harness />);

    await userEvent.click(screen.getByRole("button", { name: "Run" }));

    expect(await screen.findByText("Ocurrió un error inesperado.")).toBeInTheDocument();
  });
});
