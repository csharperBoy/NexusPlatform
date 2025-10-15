import React from "react";

const TailwindTest: React.FC = () => {
  return (
    <div className="min-h-screen flex flex-col items-center justify-center bg-red-200">
      <h1 className="text-4xl font-bold text-red-600 mb-4">
        تست Tailwind
      </h1>
      <button className="bg-blue-600 py-2 px-4 border border-transparent text-center text-sm text-white transition-all shadow-md hover:shadow-lg focus:bg-blue-700 focus:shadow-none active:bg-blue-700 hover:bg-blue-700  rounded-md active:shadow-none disabled:pointer-events-none disabled:opacity-50 disabled:shadow-none ml-2" type="submit">ورود</button>
      
    </div>
  );
};

export default TailwindTest;
