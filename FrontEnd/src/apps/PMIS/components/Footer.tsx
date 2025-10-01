//src/components/Footer.tsx
// import { DiReact } from 'react-icons/di';

const Footer = () => {
  return (
    
    <div className="w-full px-5 py-5 xl:m-0 mt-5 flex justify-between gap-2 font-semibold xl:text-sm">
      
      <span className="hidden xl:inline-flex text-sm">
        {/* <img
            src="/AkSteel-Logo.png"
            alt="AkSteel Logo"
            className="sm:w-10 sm:h-10 xl:w-15 xl:h-15 text-primary"
          /> */}
       شرکت فولاد امیرکبیر کاشان
      </span>
      
      <div className="flex gap-1 items-center">
        <span className="text-sm">© MAHAR SoftWare Group</span>
        {/* <DiReact className="text-2xl xl:text-xl 2xl:text-2xl text-primary animate-spin-slow" /> */}
      <img
            src="/MAHAR-Logo.png"
            alt="MAHAR Logo"
            className="w-4 h-4 sm:w-6 sm:h-6 xl:w-6 xl:h-6 text-primary"
          />
      </div>
    </div>
  );
};

export default Footer;
