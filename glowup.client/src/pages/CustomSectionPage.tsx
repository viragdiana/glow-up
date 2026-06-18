import { useCallback, useEffect, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { api } from '../services/api';
import { useLayoutContext } from '../layout/AppLayout';
import type { CustomSectionInput } from '../types/customSection';

interface KvRow {
  rowId: string;
  key: string;
  value: string;
}

function rowId() {
  return Math.random().toString(36).slice(2);
}

/** Parse stored dataJson into editable key/value rows. */
function parseRows(dataJson: string | undefined): KvRow[] {
  if (!dataJson) return [];
  try {
    const parsed = JSON.parse(dataJson) as Record<string, unknown>;
    return Object.entries(parsed).map(([key, value]) => ({
      rowId: rowId(),
      key,
      value: value == null ? '' : String(value),
    }));
  } catch {
    return [];
  }
}

/** Build a dataJson object from key/value rows, skipping rows without a key. */
function buildDataJson(rows: KvRow[]): string {
  const obj: Record<string, string> = {};
  for (const row of rows) {
    const key = row.key.trim();
    if (key) obj[key] = row.value;
  }
  return JSON.stringify(obj);
}

export default function CustomSectionPage() {
  const { id } = useParams<{ id: string }>();
  const isNew = !id || id === 'new';
  const navigate = useNavigate();
  const { refreshCustomSections } = useLayoutContext();

  const [title, setTitle] = useState('');
  const [summary, setSummary] = useState('');
  const [notes, setNotes] = useState('');
  const [confidence, setConfidence] = useState('');
  const [rows, setRows] = useState<KvRow[]>([]);

  const [loading, setLoading] = useState(!isNew);
  const [saving, setSaving] = useState(false);
  const [deleting, setDeleting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [saved, setSaved] = useState(false);

  const load = useCallback(async () => {
    if (isNew || !id) {
      // Reset the form when switching into "new" mode from an existing section.
      setTitle('');
      setSummary('');
      setNotes('');
      setConfidence('');
      setRows([]);
      setError(null);
      setSaved(false);
      setLoading(false);
      return;
    }
    setLoading(true);
    try {
      setError(null);
      const section = await api.getCustomSection(id);
      setTitle(section.title);
      setSummary(section.summary ?? '');
      setNotes(section.notes ?? '');
      setConfidence(section.confidenceLevel != null ? String(section.confidenceLevel) : '');
      setRows(parseRows(section.dataJson));
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to load section');
    } finally {
      setLoading(false);
    }
  }, [id, isNew]);

  useEffect(() => {
    void load();
  }, [load]);

  const markDirty = () => setSaved(false);

  const updateRow = (rid: string, patch: Partial<KvRow>) => {
    setRows((prev) => prev.map((r) => (r.rowId === rid ? { ...r, ...patch } : r)));
    markDirty();
  };
  const addRow = () => {
    setRows((prev) => [...prev, { rowId: rowId(), key: '', value: '' }]);
    markDirty();
  };
  const removeRow = (rid: string) => {
    setRows((prev) => prev.filter((r) => r.rowId !== rid));
    markDirty();
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!title.trim()) {
      setError('Title is required.');
      return;
    }
    setSaving(true);
    setError(null);

    const input: CustomSectionInput = {
      title: title.trim(),
      summary: summary || null,
      notes: notes || null,
      confidenceLevel: confidence === '' ? null : Number(confidence),
      dataJson: buildDataJson(rows),
    };

    try {
      if (isNew) {
        const created = await api.createCustomSection(input);
        await refreshCustomSections();
        navigate(`/custom-sections/${created.id}`);
      } else {
        await api.updateCustomSection(id!, input);
        await refreshCustomSections();
        setSaved(true);
      }
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to save section');
    } finally {
      setSaving(false);
    }
  };

  const handleDelete = async () => {
    if (isNew || !id) return;
    if (!window.confirm('Delete this custom section? This cannot be undone.')) return;
    setDeleting(true);
    setError(null);
    try {
      await api.deleteCustomSection(id);
      await refreshCustomSections();
      navigate('/');
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to delete section');
      setDeleting(false);
    }
  };

  return (
    <div className="page">
      <div className="page-head">
        <h2 className="page-title">
          <span className="page-title-icon">✨</span>{' '}
          {isNew ? 'New Custom Section' : title || 'Custom Section'}
        </h2>
        <p className="page-subtitle">
          {isNew
            ? 'Create your own section, e.g. Enneagram, Love Languages, or Career Values.'
            : 'Edit your custom section, or delete it below.'}
        </p>
      </div>

      <section className="card">
        {loading ? (
          <div className="muted">Loading section…</div>
        ) : (
          <form className="form" onSubmit={handleSubmit}>
            <div className="form-grid">
              <label className="field">
                <span className="field-label">Title</span>
                <input
                  className="input"
                  type="text"
                  placeholder="e.g. Enneagram"
                  value={title}
                  onChange={(e) => {
                    setTitle(e.target.value);
                    markDirty();
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
                    markDirty();
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
                  markDirty();
                }}
              />
            </label>

            <div className="section-fields">
              <div className="section-fields-head">
                <h3 className="section-fields-title">Custom Fields</h3>
                <button type="button" className="btn btn-secondary btn-sm" onClick={addRow}>
                  + Add Field
                </button>
              </div>

              {rows.length === 0 && (
                <p className="muted">
                  No fields yet. Add key/value pairs like <code>Type → 4w5</code>.
                </p>
              )}

              <div className="kv-list">
                {rows.map((row) => (
                  <div key={row.rowId} className="kv-row">
                    <input
                      className="input"
                      type="text"
                      placeholder="Field name"
                      value={row.key}
                      onChange={(e) => updateRow(row.rowId, { key: e.target.value })}
                    />
                    <input
                      className="input"
                      type="text"
                      placeholder="Value"
                      value={row.value}
                      onChange={(e) => updateRow(row.rowId, { value: e.target.value })}
                    />
                    <button
                      type="button"
                      className="btn-icon"
                      aria-label="Remove field"
                      onClick={() => removeRow(row.rowId)}
                    >
                      ✕
                    </button>
                  </div>
                ))}
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
                  markDirty();
                }}
              />
            </label>

            {error && <div className="banner banner-error">{error}</div>}

            <div className="form-actions form-actions-split">
              {!isNew && (
                <button
                  type="button"
                  className="btn btn-danger"
                  onClick={handleDelete}
                  disabled={deleting || saving}
                >
                  {deleting ? 'Deleting…' : 'Delete'}
                </button>
              )}
              <div className="form-actions">
                {saved && <span className="save-hint">Saved ✓</span>}
                <button className="btn btn-primary" type="submit" disabled={saving || deleting}>
                  {saving ? 'Saving…' : isNew ? 'Create Section' : 'Save Section'}
                </button>
              </div>
            </div>
          </form>
        )}
      </section>
    </div>
  );
}
