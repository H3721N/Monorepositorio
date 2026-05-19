export function StatusMessage({ type, children }: { type: "error" | "success" | "info"; children: React.ReactNode }) {
  return <div className={`status status-${type}`}>{children}</div>;
}
