import type { ProfileSection } from './section';

// Custom sections share the ProfileSection shape (sectionType === 'Custom').
export type CustomSection = ProfileSection;

// Body for POST/PUT /api/custom-sections — mirrors CustomSectionInputDto.
export interface CustomSectionInput {
  title: string;
  summary?: string | null;
  notes?: string | null;
  confidenceLevel?: number | null;
  dataJson: string;
}
