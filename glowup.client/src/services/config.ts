// Single place to configure where the backend API lives.
// Override at build/run time with a VITE_API_BASE_URL env variable if needed.
export const API_BASE_URL =
  import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5288';
