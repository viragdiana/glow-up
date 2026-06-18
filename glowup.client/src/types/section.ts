// Matches the backend SectionType enum (serialized as string names).
export type SectionType =
  | 'Mbti'
  | 'Kibbe'
  | 'Astrology'
  | 'HumanDesign'
  | 'BigFive'
  | 'Custom';

// Mirrors GlowUp.Api ProfileSectionDto.
export interface ProfileSection {
  id: string;
  profileId: string;
  sectionType: SectionType;
  title: string;
  summary?: string | null;
  notes?: string | null;
  confidenceLevel?: number | null;
  dataJson: string; // structured section-specific fields as a JSON string
  createdAt: string;
  updatedAt: string;
}

// Mirrors GlowUp.Api UpdateProfileSectionDto (PUT /api/sections/{sectionType} body).
export interface UpdateProfileSection {
  title: string;
  summary?: string | null;
  notes?: string | null;
  confidenceLevel?: number | null;
  dataJson: string;
}
