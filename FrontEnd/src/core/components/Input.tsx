// src/core/components/Input.tsx
import React from "react";
import clsx from "clsx";
import { useUIConfig } from "@/core/context/UIProvider";

type InputProps = React.InputHTMLAttributes<HTMLInputElement> & {
  variant?: "default" | "filled";
};

const Input: React.FC<InputProps> = ({ 
  className, 
  variant = "default",
  ...props 
}) => {
  const { theme, style } = useUIConfig();

  const baseStyles = "transition-all duration-200 focus:outline-none focus:ring-2 w-full rounded-md";
  
  const themeStyles = {
    Light: {
      default: "border border-gray-300 bg-white text-gray-900 placeholder-gray-500 focus:border-blue-500 focus:ring-blue-300",
      filled: "border border-transparent bg-gray-100 text-gray-900 placeholder-gray-500 focus:bg-white focus:border-blue-500 focus:ring-blue-300"
    },
    Dark: {
      default: "border border-gray-600 bg-gray-700 text-white placeholder-gray-400 focus:border-blue-500 focus:ring-blue-700",
      filled: "border border-transparent bg-gray-600 text-white placeholder-gray-400 focus:bg-gray-700 focus:border-blue-500 focus:ring-blue-700"
    },
    Auto: {
      default: "border border-gray-300 dark:border-gray-600 bg-white dark:bg-gray-700 text-gray-900 dark:text-white placeholder-gray-500 dark:placeholder-gray-400 focus:border-blue-500 focus:ring-blue-300 dark:focus:ring-blue-700",
      filled: "border border-transparent bg-gray-100 dark:bg-gray-600 text-gray-900 dark:text-white placeholder-gray-500 dark:placeholder-gray-400 focus:bg-white dark:focus:bg-gray-700 focus:border-blue-500 focus:ring-blue-300 dark:focus:ring-blue-700"
    }
  };

  const styleVariants = {
    Minimalist: "border-0 border-b-2 rounded-none focus:ring-0 bg-transparent",
    MaterialDesign: "shadow-sm focus:shadow-md py-3 px-0 border-0 border-b-2 rounded-none focus:ring-0",
    Flat: "shadow-inner focus:shadow-md",
    Glassmorphism: "backdrop-blur-md bg-white/20 dark:bg-black/20 border border-white/30 dark:border-gray-600/30 focus:bg-white/40 dark:focus:bg-black/40",
    Neumorphism: {
      Light: "bg-gray-100 border-0 shadow-[inset_3px_3px_6px_#d1d1d1,inset_-3px_-3px_6px_#ffffff] focus:shadow-[inset_1px_1px_2px_#d1d1d1,inset_-1px_-1px_2px_#ffffff] focus:ring-0",
      Dark: "bg-gray-800 border-0 shadow-[inset_3px_3px_6px_#1f2937,inset_-3px_-3px_6px_#374151] focus:shadow-[inset_1px_1px_2px_#1f2937,inset_-1px_-1px_2px_#374151] focus:ring-0"
    }
  };

  const getStyleVariant = () => {
    if (style === "Neumorphism") {
      const themeKey = theme === "Auto" ? "Light" : theme;
      return styleVariants.Neumorphism[themeKey as keyof typeof styleVariants.Neumorphism];
    }
    return styleVariants[style as keyof typeof styleVariants];
  };

  const paddingStyles = style === "MaterialDesign" ? "px-0" : "px-3";
  const heightStyles = style === "MaterialDesign" ? "py-3" : "py-2";

  return (
    <input
      className={clsx(
        baseStyles,
        paddingStyles,
        heightStyles,
        themeStyles[theme][variant],
        getStyleVariant(),
        className
      )}
      {...props}
    />
  );
};

export default Input;