// src/core/components/Button.tsx
import React from "react";
import clsx from "clsx";
import { useUIConfig } from "@/core/context/UIProvider";

type ButtonProps = React.ButtonHTMLAttributes<HTMLButtonElement> & {
  variant?: "primary" | "secondary" | "ghost";
  size?: "sm" | "md" | "lg";
};

const Button: React.FC<ButtonProps> = ({ 
  children, 
  className, 
  variant = "primary",
  size = "md",
  ...props 
}) => {
  const { theme, style } = useUIConfig();

  const baseStyles = "font-medium transition-all duration-200 focus:outline-none focus:ring-2 disabled:opacity-50 disabled:cursor-not-allowed";
  
  const sizes = {
    sm: "px-3 py-1.5 text-sm rounded",
    md: "px-4 py-2 text-base rounded-md",
    lg: "px-6 py-3 text-lg rounded-lg"
  };

  const themeStyles = {
    Light: {
      primary: "bg-blue-600 text-white hover:bg-blue-700 focus:ring-blue-300",
      secondary: "bg-gray-200 text-gray-800 hover:bg-gray-300 focus:ring-gray-300",
      ghost: "text-gray-600 hover:bg-gray-100 focus:ring-gray-300"
    },
    Dark: {
      primary: "bg-blue-500 text-white hover:bg-blue-600 focus:ring-blue-700",
      secondary: "bg-gray-700 text-gray-200 hover:bg-gray-600 focus:ring-gray-600",
      ghost: "text-gray-300 hover:bg-gray-800 focus:ring-gray-600"
    },
    Auto: {
      primary: "bg-blue-600 dark:bg-blue-500 text-white hover:bg-blue-700 dark:hover:bg-blue-600 focus:ring-blue-300 dark:focus:ring-blue-700",
      secondary: "bg-gray-200 dark:bg-gray-700 text-gray-800 dark:text-gray-200 hover:bg-gray-300 dark:hover:bg-gray-600 focus:ring-gray-300 dark:focus:ring-gray-600",
      ghost: "text-gray-600 dark:text-gray-300 hover:bg-gray-100 dark:hover:bg-gray-800 focus:ring-gray-300 dark:focus:ring-gray-600"
    }
  };

  const styleVariants = {
    Minimalist: "border-0 shadow-none",
    MaterialDesign: "shadow-md hover:shadow-lg active:shadow-sm",
    Flat: "shadow-sm hover:shadow-md active:shadow-none",
    Glassmorphism: "backdrop-blur-md bg-white/20 dark:bg-black/20 border border-white/30 dark:border-gray-600/30",
    Neumorphism: {
      Light: "bg-gray-100 shadow-[5px_5px_10px_#d1d1d1,-5px_-5px_10px_#ffffff] active:shadow-[inset_5px_5px_10px_#d1d1d1,inset_-5px_-5px_10px_#ffffff]",
      Dark: "bg-gray-700 shadow-[5px_5px_10px_#1f2937,-5px_-5px_10px_#374151] active:shadow-[inset_5px_5px_10px_#1f2937,inset_-5px_-5px_10px_#374151]"
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
    <button
      className={clsx(
        baseStyles,
        sizes[size],
        themeStyles[theme][variant],
        getStyleVariant(),
        className
      )}
      {...props}
    >
      {children}
    </button>
  );
};

export default Button;