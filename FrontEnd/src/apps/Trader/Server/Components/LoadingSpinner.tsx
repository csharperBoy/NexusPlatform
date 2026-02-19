// src/apps/Trader/Server/Components/LoadingSpinner.tsx
import React from 'react';


const LoadingSpinner: React.FC = () => {
  return (
    <div className="flex items-center justify-center h-screen bg-gray-100">
    <div className="text-2xl text-purple-600 animate-pulse">در حال آماده‌سازی...</div>
  </div>
  );
};

export default LoadingSpinner;