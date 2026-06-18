import { useRef, useState } from 'react';
import { api } from '../services/api';

interface ChatMessage {
  id: string;
  role: 'user' | 'bot';
  text: string;
}

const EXAMPLE_QUESTIONS = [
  'Summarize my full self-knowledge profile.',
  'Based on my MBTI and Big Five, how should I study?',
  'How do my Kibbe and personality profile combine for personal style?',
  'What career directions fit my profile?',
  'What should I focus on for personal growth?',
];

function newId() {
  return Math.random().toString(36).slice(2);
}

export default function AiChatPage() {
  const [messages, setMessages] = useState<ChatMessage[]>([]);
  const [input, setInput] = useState('');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const listEndRef = useRef<HTMLDivElement | null>(null);

  const scrollToBottom = () => {
    requestAnimationFrame(() => {
      listEndRef.current?.scrollIntoView({ behavior: 'smooth' });
    });
  };

  const ask = async (question: string) => {
    const trimmed = question.trim();
    if (!trimmed || loading) return;

    setError(null);
    setInput('');
    setMessages((prev) => [...prev, { id: newId(), role: 'user', text: trimmed }]);
    scrollToBottom();
    setLoading(true);

    try {
      const res = await api.sendAiChatQuestion(trimmed);
      setMessages((prev) => [...prev, { id: newId(), role: 'bot', text: res.answer }]);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to get an answer');
    } finally {
      setLoading(false);
      scrollToBottom();
    }
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    void ask(input);
  };

  return (
    <div className="page">
      <div className="page-head">
        <h2 className="page-title">
          <span className="page-title-icon">💬</span> AI Chat
        </h2>
        <p className="page-subtitle">
          Answers are generated from your saved Glow Up profile data.
        </p>
      </div>

      <section className="card chat-card">
        <div className="chat-history">
          {messages.length === 0 && !loading && (
            <div className="chat-empty">
              <p className="muted">Ask a question about yourself to get started. Try one of these:</p>
              <div className="chat-suggestions">
                {EXAMPLE_QUESTIONS.map((q) => (
                  <button
                    key={q}
                    type="button"
                    className="chip"
                    onClick={() => void ask(q)}
                  >
                    {q}
                  </button>
                ))}
              </div>
            </div>
          )}

          {messages.map((m) => (
            <div key={m.id} className={`chat-row chat-row-${m.role}`}>
              <div className={`chat-bubble chat-bubble-${m.role}`}>{m.text}</div>
            </div>
          ))}

          {loading && (
            <div className="chat-row chat-row-bot">
              <div className="chat-bubble chat-bubble-bot chat-bubble-loading">
                <span className="dot" />
                <span className="dot" />
                <span className="dot" />
              </div>
            </div>
          )}

          <div ref={listEndRef} />
        </div>

        {error && <div className="banner banner-error chat-error">{error}</div>}

        <form className="chat-input-row" onSubmit={handleSubmit}>
          <input
            className="input"
            type="text"
            placeholder="Ask about your profile…"
            value={input}
            onChange={(e) => setInput(e.target.value)}
            disabled={loading}
          />
          <button className="btn btn-primary" type="submit" disabled={loading || !input.trim()}>
            {loading ? 'Thinking…' : 'Send'}
          </button>
        </form>
      </section>
    </div>
  );
}
