import { useCallback, useEffect, useState } from 'react';
import { useParams } from 'react-router-dom';
import { api } from '../services/api';
import { getSectionConfig } from '../sectionConfig';
import type { ProfileSection } from '../types/section';
import SectionEditForm from '../components/SectionEditForm';

export default function SectionPage() {
  const { sectionType } = useParams<{ sectionType: string }>();
  const config = getSectionConfig(sectionType);

  const [section, setSection] = useState<ProfileSection | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const loadSection = useCallback(async () => {
    if (!config) return;
    setLoading(true);
    try {
      setError(null);
      const data = await api.getSection(config.type);
      setSection(data); // null means "not created yet"
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to load section');
    } finally {
      setLoading(false);
    }
  }, [config]);

  useEffect(() => {
    void loadSection();
  }, [loadSection]);

  if (!config) {
    return (
      <div className="page">
        <div className="banner banner-error">Unknown section: {sectionType}</div>
      </div>
    );
  }

  return (
    <div className="page">
      <div className="page-head">
        <h2 className="page-title">
          <span className="page-title-icon">{config.icon}</span> {config.label}
        </h2>
        <p className="page-subtitle">
          {section ? 'Edit your saved details below.' : 'This section is empty — fill it in to get started.'}
        </p>
      </div>

      <section className="card">
        {loading && <div className="muted">Loading section…</div>}
        {error && <div className="banner banner-error">{error}</div>}
        {!loading && !error && (
          <SectionEditForm
            // Re-mount when the section type changes so form state resets.
            key={config.type}
            config={config}
            section={section}
            onSaved={(saved) => setSection(saved)}
          />
        )}
      </section>
    </div>
  );
}
