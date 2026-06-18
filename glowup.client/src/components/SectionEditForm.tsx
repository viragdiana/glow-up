import { useState } from 'react';
import { api } from '../services/api';
import type { SectionConfig } from '../sectionConfig';
import type { ProfileSection, UpdateProfileSection } from '../types/section';

interface SectionEditFormProps {
  config: SectionConfig;
  section: ProfileSection | null; // null when the section does not exist yet
  onSaved: (saved: ProfileSection) => void | Promise<void>;
}

/** Safely parse the stored dataJson into a flat string map for the inputs. */
function parseDataJson(dataJson: string | undefined): Record<string, string> {
  if (!dataJson) return {};
  try {
    const parsed = JSON.parse(dataJson) as Record<string, unknown>;
    const result: Record<string, string> = {};
    for (const [key, value] of Object.entries(parsed)) {
      result[key] = value == null ? '' : String(value);
    }
    return result;
  } catch {
    return {};
  }
}

export default function SectionEditForm({ config, section, onSaved }: SectionEditFormProps) {
  const [title, setTitle] = useState(section?.title ?? config.label);
  const [summary, setSummary] = useState(section?.summary ?? '');
  const [notes, setNotes] = useState(section?.notes ?? '');
  const [confidence, setConfidence] = useState<string>(
    section?.confidenceLevel != null ? String(section.confidenceLevel) : '',
  );
  const [data, setData] = useState<Record<string, string>>(parseDataJson(section?.dataJson));

  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [saved, setSaved] = useState(false);

  const updateField = (key: string, value: string) => {
    setData((prev) => ({ ...prev, [key]: value }));
    setSaved(false);
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setSaving(true);
    setError(null);

    // Build the structured payload, converting number fields appropriately.
    const payloadData: Record<string, string | number | null> = {};
    for (const field of config.fields) {
      const raw = (data[field.key] ?? '').trim();
      if (field.kind === 'number') {
        payloadData[field.key] = raw === '' ? null : Number(raw);
      } else {
        payloadData[field.key] = raw;
      }
    }

    const body: UpdateProfileSection = {
      title: title.trim() || config.label,
      summary: summary || null,
      notes: notes || null,
      confidenceLevel: confidence === '' ? null : Number(confidence),
      dataJson: JSON.stringify(payloadData),
    };

    try {
      const result = await api.updateSection(config.type, body);
      setSaved(true);
      await onSaved(result);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to save section');
    } finally {
      setSaving(false);
    }
  };

  return (
    <form className="form" onSubmit={handleSubmit}>
      <div className="form-grid">
        <label className="field">
          <span className="field-label">Title</span>
          <input
            className="input"
            type="text"
            value={title}
            onChange={(e) => {
              setTitle(e.target.value);
              setSaved(false);
            }}
          />
        </label>

        <label className="field">
          <span className="field-label">Confidence Level (0–100)</span>
          <input
            className="input"
            type="number"
            min={0}
            max={100}
            value={confidence}
            onChange={(e) => {
              setConfidence(e.target.value);
              setSaved(false);
            }}
          />
        </label>
      </div>

      <label className="field">
        <span className="field-label">Summary</span>
        <textarea
          className="input textarea"
          rows={2}
          value={summary}
          onChange={(e) => {
            setSummary(e.target.value);
            setSaved(false);
          }}
        />
      </label>

      <div className="section-fields">
        <h3 className="section-fields-title">{config.label} Details</h3>
        <div className="form-grid">
          {config.fields.map((field) =>
            field.kind === 'textarea' ? (
              <label key={field.key} className="field field-full">
                <span className="field-label">{field.label}</span>
                <textarea
                  className="input textarea"
                  rows={3}
                  value={data[field.key] ?? ''}
                  onChange={(e) => updateField(field.key, e.target.value)}
                />
              </label>
            ) : (
              <label key={field.key} className="field">
                <span className="field-label">{field.label}</span>
                <input
                  className="input"
                  type={field.kind === 'number' ? 'number' : 'text'}
                  value={data[field.key] ?? ''}
                  onChange={(e) => updateField(field.key, e.target.value)}
                />
              </label>
            ),
          )}
        </div>
      </div>

      <label className="field">
        <span className="field-label">Notes</span>
        <textarea
          className="input textarea"
          rows={4}
          value={notes}
          onChange={(e) => {
            setNotes(e.target.value);
            setSaved(false);
          }}
        />
      </label>

      {error && <div className="banner banner-error">{error}</div>}

      <div className="form-actions">
        <button className="btn btn-primary" type="submit" disabled={saving}>
          {saving ? 'Saving…' : 'Save Section'}
        </button>
        {saved && <span className="save-hint">Saved ✓</span>}
      </div>
    </form>
  );
}
