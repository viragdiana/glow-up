import { useCallback, useEffect, useState } from 'react';
import { api } from '../services/api';
import type { AiContext } from '../types/aiContext';

export default function AiContextPreviewPage() {
  const [context, setContext] = useState<AiContext | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const loadContext = useCallback(async () => {
    setLoading(true);
    try {
      setError(null);
      const data = await api.getAiContext();
      setContext(data);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to load AI context');
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    void loadContext();
  }, [loadContext]);

  return (
    <div className="page">
      <div className="page-head">
        <h2 className="page-title">
          <span className="page-title-icon">🤖</span> AI Context Preview
        </h2>
        <p className="page-subtitle">
          This is the plain-text context the app will hand to an AI model later.
          No AI is called yet.
        </p>
      </div>

      <div className="form-actions form-actions-start">
        <button className="btn btn-secondary" onClick={() => void loadContext()} disabled={loading}>
          {loading ? 'Refreshing…' : 'Refresh'}
        </button>
        {context && (
          <span className="muted">
            {context.sections.length} section{context.sections.length === 1 ? '' : 's'} included
          </span>
        )}
      </div>

      {loading && <div className="muted">Building context…</div>}
      {error && <div className="banner banner-error">{error}</div>}

      {!loading && !error && context && (
        <section className="card">
          <h3 className="card-title">Generated Summary</h3>
          <pre className="context-preview">{context.generatedSummary}</pre>
        </section>
      )}
    </div>
  );
}
