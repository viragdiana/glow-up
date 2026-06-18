import type { SectionType } from './types/section';

export type FieldKind = 'text' | 'textarea' | 'number';

export interface FieldDef {
  key: string;
  label: string;
  kind: FieldKind;
}

export interface SectionConfig {
  type: SectionType;
  label: string; // display name in the UI
  icon: string; // simple emoji glyph, no icon library yet
  fields: FieldDef[]; // section-specific fields stored inside dataJson
}

// Order here drives both the sidebar and the dashboard cards.
export const SECTION_CONFIGS: SectionConfig[] = [
  {
    type: 'Mbti',
    label: 'MBTI',
    icon: '🧩',
    fields: [
      { key: 'type', label: 'Type', kind: 'text' },
      { key: 'cognitiveFunctionsNotes', label: 'Cognitive Functions Notes', kind: 'textarea' },
      { key: 'strengths', label: 'Strengths', kind: 'textarea' },
      { key: 'challenges', label: 'Challenges', kind: 'textarea' },
    ],
  },
  {
    type: 'Kibbe',
    label: 'Kibbe Body Type',
    icon: '👗',
    fields: [
      { key: 'bodyType', label: 'Body Type', kind: 'text' },
      { key: 'essenceNotes', label: 'Essence Notes', kind: 'textarea' },
      { key: 'styleRecommendations', label: 'Style Recommendations', kind: 'textarea' },
      { key: 'avoidNotes', label: 'Avoid', kind: 'textarea' },
    ],
  },
  {
    type: 'Astrology',
    label: 'Astrology',
    icon: '🌙',
    fields: [
      { key: 'sunSign', label: 'Sun Sign', kind: 'text' },
      { key: 'moonSign', label: 'Moon Sign', kind: 'text' },
      { key: 'risingSign', label: 'Rising Sign', kind: 'text' },
      { key: 'venusSign', label: 'Venus Sign', kind: 'text' },
      { key: 'marsSign', label: 'Mars Sign', kind: 'text' },
      { key: 'notes', label: 'Notes', kind: 'textarea' },
    ],
  },
  {
    type: 'HumanDesign',
    label: 'Human Design',
    icon: '🔆',
    fields: [
      { key: 'type', label: 'Type', kind: 'text' },
      { key: 'strategy', label: 'Strategy', kind: 'text' },
      { key: 'authority', label: 'Authority', kind: 'text' },
      { key: 'profile', label: 'Profile', kind: 'text' },
      { key: 'centersNotes', label: 'Centers Notes', kind: 'textarea' },
    ],
  },
  {
    type: 'BigFive',
    label: 'Big Five',
    icon: '📊',
    fields: [
      { key: 'openness', label: 'Openness', kind: 'number' },
      { key: 'conscientiousness', label: 'Conscientiousness', kind: 'number' },
      { key: 'extraversion', label: 'Extraversion', kind: 'number' },
      { key: 'agreeableness', label: 'Agreeableness', kind: 'number' },
      { key: 'neuroticism', label: 'Neuroticism', kind: 'number' },
      { key: 'interpretationNotes', label: 'Interpretation Notes', kind: 'textarea' },
    ],
  },
];

export function getSectionConfig(type: string | undefined): SectionConfig | undefined {
  return SECTION_CONFIGS.find((c) => c.type === type);
}
