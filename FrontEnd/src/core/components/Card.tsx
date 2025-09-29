// src/core/components/Card.tsx
import React, { ReactNode } from "react";
import clsx from "clsx";
import { useUIConfig } from "@/core/context/UIProvider";

type CardProps = {
  children: ReactNode;
  className?: string;
  padding?: "none" | "sm" | "md" | "lg";
};

const Card: React.FC<CardProps> = ({ 
  children, 
  className, 
  padding = "md" 
}) => {
  const { theme, style } = useUIConfig();

  const baseStyles = "transition-all duration-200 rounded-lg";
  
  const paddings = {
    none: "p-0",
    sm: "p-3",
    md: "p-6",
    lg: "p-8"
  };

  const themeStyles = {
    Light: "bg-white text-gray-900 border border-gray-200",
    Dark: "bg-gray-800 text-white border border-gray-700",
    Auto: "bg-white dark:bg-gray-800 text-gray-900 dark:text-white border border-gray-200 dark:border-gray-700"
  };

  const styleVariants = {
    Minimalist: "border-0 shadow-none bg-transparent",
    MaterialDesign: "shadow-lg hover:shadow-xl",
    Flat: "shadow-md border-0",
    Glassmorphism: "backdrop-blur-lg bg-white/30 dark:bg-black/30 border border-white/40 dark:border-gray-600/40",
    Neumorphism: {
      Light: "bg-gray-100 border-0 shadow-[8px_8px_16px_#d1d1d1,-8px_-8px_16px_#ffffff]",
      Dark: "bg-gray-800 border-0 shadow-[8px_8px_16px_#1f2937,-8px_-8px_16px_#374151]"
    }
  };

  const getStyleVariant = () => {
    if (style === "Neumorphism") {
      const themeKey = theme === "Auto" ? "Light" : theme;
      return styleVariants.Neumorphism[themeKey as keyof typeof styleVariants.Neumorphism];
    }
    return styleVariants[style as keyof typeof styleVariants];
  };

  return (
    <div
      className={clsx(
        baseStyles,
        paddings[padding],
        themeStyles[theme],
        getStyleVariant(),
        className
      )}
    >
      {children}
    </div>
  );
};

export default Card;