import { useState } from 'react';
import { api } from '../services/api';
import type { Profile, UpdateProfile } from '../types/profile';

interface ProfileEditFormProps {
  profile: Profile;
  onSaved: () => void | Promise<void>;
}

export default function ProfileEditForm({ profile, onSaved }: ProfileEditFormProps) {
  const [form, setForm] = useState<UpdateProfile>({
    displayName: profile.displayName,
    bio: profile.bio ?? '',
    avatarUrl: profile.avatarUrl ?? '',
    birthDate: profile.birthDate ?? '',
    location: profile.location ?? '',
    generalNotes: profile.generalNotes ?? '',
  });
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [savedAt, setSavedAt] = useState<number | null>(null);

  const update = <K extends keyof UpdateProfile>(key: K, value: UpdateProfile[K]) => {
    setForm((prev) => ({ ...prev, [key]: value }));
    setSavedAt(null);
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setSaving(true);
    setError(null);
    try {
      await api.updateProfile({
        ...form,
        // send null instead of empty strings for optional fields
        bio: form.bio || null,
        avatarUrl: form.avatarUrl || null,
        birthDate: form.birthDate || null,
        location: form.location || null,
        generalNotes: form.generalNotes || null,
      });
      setSavedAt(Date.now());
      await onSaved();
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to save profile');
    } finally {
      setSaving(false);
    }
  };

  return (
    <form className="form" onSubmit={handleSubmit}>
      <div className="form-grid">
        <label className="field">
          <span className="field-label">Display Name</span>
          <input
            className="input"
            type="text"
            required
            value={form.displayName}
            onChange={(e) => update('displayName', e.target.value)}
          />
        </label>

        <label className="field">
          <span className="field-label">Location</span>
          <input
            className="input"
            type="text"
            value={form.location ?? ''}
            onChange={(e) => update('location', e.target.value)}
          />
        </label>

        <label className="field">
          <span className="field-label">Birth Date</span>
          <input
            className="input"
            type="date"
            value={form.birthDate ?? ''}
            onChange={(e) => update('birthDate', e.target.value)}
          />
        </label>

        <label className="field">
          <span className="field-label">Avatar URL</span>
          <input
            className="input"
            type="url"
            placeholder="https://…"
            value={form.avatarUrl ?? ''}
            onChange={(e) => update('avatarUrl', e.target.value)}
          />
        </label>
      </div>

      <label className="field">
        <span className="field-label">Bio</span>
        <textarea
          className="input textarea"
          rows={3}
          value={form.bio ?? ''}
          onChange={(e) => update('bio', e.target.value)}
        />
      </label>

      <label className="field">
        <span className="field-label">General Notes</span>
        <textarea
          className="input textarea"
          rows={4}
          value={form.generalNotes ?? ''}
          onChange={(e) => update('generalNotes', e.target.value)}
        />
      </label>

      {error && <div className="banner banner-error">{error}</div>}

      <div className="form-actions">
        <button className="btn btn-primary" type="submit" disabled={saving}>
          {saving ? 'Saving…' : 'Save Profile'}
        </button>
        {savedAt && <span className="save-hint">Saved ✓</span>}
      </div>
    </form>
  );
}
