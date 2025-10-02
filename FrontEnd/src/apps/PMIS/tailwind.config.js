// src/apps/PMIS/tailwind.config.js
/** @type {import('tailwindcss').Config} */
export default {
  content: [
    './index.html',
    './**/*.{js,ts,jsx,tsx}',
    // مسیرهای مشخص برای کامپوننت‌های مشترک
    // '../**/src/**/*.{js,ts,jsx,tsx}', // سایر اپ‌ها
    // '../../../core/**/*.{js,ts,jsx,tsx}', // کامپوننت‌های core
    // '../../../modules/**/*.{js,ts,jsx,tsx}', // ماژول‌ها
  ],
  theme: {
    extend: {
      animation: {
        'spin-slow': 'spin 7s linear infinite',
      },
      screens: {
        '3xl': '2200px',
      },
    },
  },
  plugins: [require('daisyui')],
  daisyui: {
    themes: ['light', 'dark'],
  },
  darkMode: ['class', '[data-theme="dark"]'],
};