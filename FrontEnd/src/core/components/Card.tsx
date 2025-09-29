// src/core/components/Card.tsx
import React, { ReactNode } from "react";
import clsx from "clsx";
import { useUIConfig } from "@/core/context/UIProvider";

type CardProps = {
  children: ReactNode;
  className?: string;
};

const Card: React.FC<CardProps> = ({ children, className }) => {
  const { theme, style } = useUIConfig();

  // رنگ و تم
  const themeStyles: Record<string, string> = {
    light: "bg-white text-gray-800 border-gray-200",
    dark: "bg-gray-900 text-white border-gray-700",
    admin: "bg-purple-900 text-white border-purple-700",
  };

  // استایل‌ها
  const styleVariants: Record<string, string> = {
    flat: "shadow-none",
    "3d": "shadow-xl border rounded-xl",
    glass: "backdrop-blur-md bg-white/30 border border-white/20 rounded-xl",
  };

  return (
    <div
      className={clsx(
        "p-6 border transition",
        themeStyles[theme],
        styleVariants[style],
        className
      )}
    >
      {children}
    </div>
  );
};

export default Card;
