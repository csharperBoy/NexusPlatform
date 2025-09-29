// src/core/components/Button.tsx
import React from "react";
import clsx from "clsx";
import { useUIConfig } from "@/core/context/UIProvider";

type ButtonProps = React.ButtonHTMLAttributes<HTMLButtonElement>;

const Button: React.FC<ButtonProps> = ({ children, className, ...props }) => {
  const { theme, style } = useUIConfig();

  // رنگ و تم
  const themeStyles: Record<string, string> = {
    light: "bg-blue-600 text-white hover:bg-blue-700",
    dark: "bg-gray-700 text-white hover:bg-gray-600",
    admin: "bg-purple-600 text-white hover:bg-purple-700",
  };

  // نوع استایل (Flat, 3D, Glass)
  const styleVariants: Record<string, string> = {
    flat: "shadow-none",
    "3d": "shadow-lg border border-gray-300",
    glass: "backdrop-blur-md bg-white/30 border border-white/20",
  };

  return (
    <button
      className={clsx(
        "px-4 py-2 rounded-lg font-medium transition",
        themeStyles[theme],
        styleVariants[style],
        className
      )}
      {...props}
    >
      {children}
    </button>
  );
};

export default Button;
