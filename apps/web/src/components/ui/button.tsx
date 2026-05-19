import { Slot } from "@radix-ui/react-slot";
import { cva, type VariantProps } from "class-variance-authority";
import type { ButtonHTMLAttributes } from "react";
import { cn } from "../../utils/cn";

const buttonVariants = cva("inline-flex items-center justify-center gap-2 rounded-md font-medium transition-colors", {
  variants: {
    variant: {
      primary: "bg-slate-900 text-white hover:bg-slate-700",
      secondary: "border border-slate-300 bg-white text-slate-900 hover:bg-slate-100",
      danger: "bg-red-600 text-white hover:bg-red-700"
    },
    size: {
      sm: "h-8 px-3 text-sm",
      md: "h-10 px-4 text-sm"
    }
  },
  defaultVariants: {
    variant: "primary",
    size: "md"
  }
});

interface ButtonProps extends ButtonHTMLAttributes<HTMLButtonElement>, VariantProps<typeof buttonVariants> {
  asChild?: boolean;
}

export function Button({ asChild, className, variant, size, ...props }: ButtonProps) {
  const Comp = asChild ? Slot : "button";
  return <Comp className={cn(buttonVariants({ variant, size }), className)} {...props} />;
}
