import { useCallback, useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { api } from '../services/api';
import { SECTION_CONFIGS } from '../sectionConfig';
import type { ProfileSection } from '../types/section';
import { useLayoutContext } from '../layout/AppLayout';
import ProfileEditForm from '../components/ProfileEditForm';

export default function DashboardPage() {
  const { profile, refreshProfile } = useLayoutContext();

  const [sections, setSections] = useState<ProfileSection[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const loadSections = useCallback(async () => {
    try {
      setError(null);
      const data = await api.getSections();
      setSections(data);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to load sections');
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    void loadSections();
  }, [loadSections]);

  const sectionByType = new Map(sections.map((s) => [s.sectionType, s]));

  return (
    <div className="page">
      <div className="page-head">
        <h2 className="page-title">Dashboard</h2>
        <p className="page-subtitle">
          Your self-knowledge hub. Edit your profile and track each section.
        </p>
      </div>

      <section className="card">
        <h3 className="card-title">Profile</h3>
        {profile ? (
          <ProfileEditForm profile={profile} onSaved={refreshProfile} />
        ) : (
          <div className="muted">Loading profile…</div>
        )}
      </section>

      <section>
        <h3 className="section-heading">Sections</h3>
        {loading && <div className="muted">Loading sections…</div>}
        {error && <div className="banner banner-error">{error}</div>}
        {!loading && !error && (
          <div className="card-grid">
            {SECTION_CONFIGS.map((config) => {
              const filled = sectionByType.get(config.type);
              return (
                <Link
                  key={config.type}
                  to={`/sections/${config.type}`}
                  className="section-card"
                >
                  <div className="section-card-icon">{config.icon}</div>
                  <div className="section-card-body">
                    <div className="section-card-label">{config.label}</div>
                    <div className={`badge ${filled ? 'badge-done' : 'badge-empty'}`}>
                      {filled ? 'Saved' : 'Not started'}
                    </div>
                    {filled?.summary && (
                      <p className="section-card-summary">{filled.summary}</p>
                    )}
                  </div>
                </Link>
              );
            })}
          </div>
        )}
      </section>
    </div>
  );
}
