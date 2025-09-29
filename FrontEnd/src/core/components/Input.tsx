// src/core/components/Input.tsx
import React from "react";
import clsx from "clsx";
import { useUIConfig } from "@/core/context/UIProvider";

type InputProps = React.InputHTMLAttributes<HTMLInputElement>;

const Input: React.FC<InputProps> = ({ className, ...props }) => {
  const { theme, style } = useUIConfig();

  // رنگ و تم
  const themeStyles: Record<string, string> = {
    light: "border-gray-300 focus:ring-blue-500",
    dark: "border-gray-600 bg-gray-800 text-white focus:ring-gray-400",
    admin: "border-purple-400 focus:ring-purple-500",
  };

  // استایل‌ها
  const styleVariants: Record<string, string> = {
    flat: "shadow-none",
    "3d": "shadow-md border rounded-md",
    glass: "backdrop-blur-md bg-white/30 border border-white/20",
  };

  return (
    <input
      className={clsx(
        "w-full px-4 py-2 rounded-lg border focus:outline-none focus:ring-2 transition",
        themeStyles[theme],
        styleVariants[style],
        className
      )}
      {...props}
    />
  );
};

export default Input;
