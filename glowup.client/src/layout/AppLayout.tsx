import { useCallback, useEffect, useState } from 'react';
import { Outlet, useOutletContext } from 'react-router-dom';
import { api } from '../services/api';
import type { Profile } from '../types/profile';
import type { CustomSection } from '../types/customSection';
import Sidebar from './Sidebar';
import ProfileHeader from './ProfileHeader';

// Shared with child routes via react-router's Outlet context.
export interface LayoutContext {
  profile: Profile | null;
  refreshProfile: () => Promise<void>;
  customSections: CustomSection[];
  refreshCustomSections: () => Promise<void>;
}

export function useLayoutContext() {
  return useOutletContext<LayoutContext>();
}

export default function AppLayout() {
  const [profile, setProfile] = useState<Profile | null>(null);
  const [customSections, setCustomSections] = useState<CustomSection[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const loadProfile = useCallback(async () => {
    try {
      setError(null);
      const data = await api.getProfile();
      setProfile(data);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to load profile');
    } finally {
      setLoading(false);
    }
  }, []);

  const loadCustomSections = useCallback(async () => {
    try {
      const data = await api.getCustomSections();
      setCustomSections(data);
    } catch {
      // Sidebar custom list is non-critical; ignore load errors here.
    }
  }, []);

  useEffect(() => {
    void loadProfile();
    void loadCustomSections();
  }, [loadProfile, loadCustomSections]);

  return (
    <div className="app-shell">
      <Sidebar customSections={customSections} />
      <div className="app-main">
        <ProfileHeader profile={profile} loading={loading} />
        <main className="app-content">
          {error && <div className="banner banner-error">{error}</div>}
          <Outlet
            context={
              {
                profile,
                refreshProfile: loadProfile,
                customSections,
                refreshCustomSections: loadCustomSections,
              } satisfies LayoutContext
            }
          />
        </main>
      </div>
    </div>
  );
}
