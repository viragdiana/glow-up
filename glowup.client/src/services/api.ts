import { API_BASE_URL } from './config';
import type { Profile, UpdateProfile } from '../types/profile';
import type { ProfileSection, UpdateProfileSection, SectionType } from '../types/section';
import type { AiContext } from '../types/aiContext';
import type { AiChatResponse } from '../types/aiChat';

/**
 * Thin fetch wrapper. Centralizes the base URL, JSON headers, and error handling
 * so individual calls stay tiny and consistent.
 */
async function request<T>(path: string, options?: RequestInit): Promise<T> {
  let res: Response;
  try {
    res = await fetch(`${API_BASE_URL}${path}`, {
      headers: { 'Content-Type': 'application/json' },
      ...options,
    });
  } catch {
    throw new Error(
      `Could not reach the API at ${API_BASE_URL}. Is the backend running?`,
    );
  }

  if (!res.ok) {
    const body = await res.text().catch(() => '');
    throw new Error(body || `Request failed with status ${res.status}`);
  }

  if (res.status === 204) {
    return undefined as T;
  }

  return (await res.json()) as T;
}

export const api = {
  // --- Profile ---
  getProfile: () => request<Profile>('/api/profile'),

  updateProfile: (data: UpdateProfile) =>
    request<Profile>('/api/profile', {
      method: 'PUT',
      body: JSON.stringify(data),
    }),

  // --- Sections ---
  getSections: () => request<ProfileSection[]>('/api/sections'),

  /** Returns null when the section has not been created yet (404). */
  getSection: async (sectionType: SectionType): Promise<ProfileSection | null> => {
    let res: Response;
    try {
      res = await fetch(`${API_BASE_URL}/api/sections/${sectionType}`, {
        headers: { 'Content-Type': 'application/json' },
      });
    } catch {
      throw new Error(
        `Could not reach the API at ${API_BASE_URL}. Is the backend running?`,
      );
    }
    if (res.status === 404) return null;
    if (!res.ok) {
      const body = await res.text().catch(() => '');
      throw new Error(body || `Request failed with status ${res.status}`);
    }
    return (await res.json()) as ProfileSection;
  },

  updateSection: (sectionType: SectionType, data: UpdateProfileSection) =>
    request<ProfileSection>(`/api/sections/${sectionType}`, {
      method: 'PUT',
      body: JSON.stringify(data),
    }),

  // --- AI context preview ---
  getAiContext: () => request<AiContext>('/api/ai-context'),

  // --- AI chat ---
  sendAiChatQuestion: (question: string) =>
    request<AiChatResponse>('/api/ai-chat', {
      method: 'POST',
      body: JSON.stringify({ question }),
    }),
};
