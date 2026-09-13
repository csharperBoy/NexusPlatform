import { CapacitorConfig } from '@capacitor/cli';

const config: CapacitorConfig = {
  appId: 'com.aksteel.admin',
  appName: 'AKSteel Admin',
  webDir: 'dist/AKSteelAdmin',
  plugins: {
    CapacitorCookies: {
      enabled: true,
    },
    CapacitorHttp: {
      enabled: true,
    }
  },
  server: {
    androidScheme: 'https',
    cleartext: true
  }
};

export default config;