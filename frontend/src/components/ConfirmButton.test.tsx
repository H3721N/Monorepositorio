import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { vi } from "vitest";
import { ConfirmButton } from "./ConfirmButton";

describe("ConfirmButton", () => {
  it("runs action when the user confirms", async () => {
    const onConfirm = vi.fn();
    vi.spyOn(window, "confirm").mockReturnValue(true);

    render(
      <ConfirmButton message="Confirm?" onConfirm={onConfirm}>
        Delete
      </ConfirmButton>
    );

    await userEvent.click(screen.getByRole("button", { name: "Delete" }));

    expect(onConfirm).toHaveBeenCalled();
  });

  it("does not run action when the user cancels", async () => {
    const onConfirm = vi.fn();
    vi.spyOn(window, "confirm").mockReturnValue(false);

    render(
      <ConfirmButton message="Confirm?" onConfirm={onConfirm}>
        Delete
      </ConfirmButton>
    );

    await userEvent.click(screen.getByRole("button", { name: "Delete" }));

    expect(onConfirm).not.toHaveBeenCalled();
  });
});
