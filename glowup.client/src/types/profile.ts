// Mirrors GlowUp.Api ProfileDto.
export interface Profile {
  id: string;
  displayName: string;
  bio?: string | null;
  avatarUrl?: string | null;
  birthDate?: string | null; // DateOnly -> "yyyy-MM-dd"
  location?: string | null;
  generalNotes?: string | null;
  createdAt: string;
  updatedAt: string;
}

// Mirrors GlowUp.Api UpdateProfileDto (PUT /api/profile body).
export interface UpdateProfile {
  displayName: string;
  bio?: string | null;
  avatarUrl?: string | null;
  birthDate?: string | null;
  location?: string | null;
  generalNotes?: string | null;
}
