// Single place to configure where the backend API lives.
//
// - Local dev: defaults to the .NET dev server on http://localhost:5288.
// - Production: defaults to '' (same origin), because the built frontend is
//   served by the .NET backend from wwwroot, so the API lives at the same URL.
// - Override at build time with a VITE_API_BASE_URL env variable if you ever
//   host the frontend and backend separately.
const fromEnv = import.meta.env.VITE_API_BASE_URL as string | undefined;

export const API_BASE_URL =
  fromEnv ?? (import.meta.env.DEV ? 'http://localhost:5288' : '');
