import type { Profile } from './profile';
import type { ProfileSection } from './section';

// Mirrors GlowUp.Api AiContextDto.
export interface AiContext {
  profile: Profile;
  sections: ProfileSection[];
  generatedSummary: string;
}
