import type { Profile } from '../types/profile';

interface ProfileHeaderProps {
  profile: Profile | null;
  loading: boolean;
}

function initials(name: string): string {
  const parts = name.trim().split(/\s+/).filter(Boolean);
  if (parts.length === 0) return '?';
  return parts
    .slice(0, 2)
    .map((p) => p[0]!.toUpperCase())
    .join('');
}

export default function ProfileHeader({ profile, loading }: ProfileHeaderProps) {
  return (
    <header className="profile-header">
      <div className="profile-header-identity">
        <div className="avatar">
          {profile?.avatarUrl ? (
            <img src={profile.avatarUrl} alt={profile.displayName} />
          ) : (
            <span>{loading ? '…' : initials(profile?.displayName ?? '?')}</span>
          )}
        </div>
        <div className="profile-header-text">
          <h1 className="profile-header-name">
            {loading ? 'Loading…' : (profile?.displayName || 'Unnamed Profile')}
          </h1>
          {profile?.location && (
            <p className="profile-header-meta">📍 {profile.location}</p>
          )}
        </div>
      </div>
    </header>
  );
}
