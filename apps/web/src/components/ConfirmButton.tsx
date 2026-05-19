interface ConfirmButtonProps {
  message: string;
  children: React.ReactNode;
  className?: string;
  disabled?: boolean;
  onConfirm: () => void | Promise<void>;
}

export function ConfirmButton({ message, children, className, disabled, onConfirm }: ConfirmButtonProps) {
  return (
    <button
      type="button"
      className={className}
      disabled={disabled}
      onClick={() => {
        if (window.confirm(message)) {
          void onConfirm();
        }
      }}
    >
      {children}
    </button>
  );
}
