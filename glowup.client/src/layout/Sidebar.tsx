import { NavLink } from 'react-router-dom';
import { SECTION_CONFIGS } from '../sectionConfig';

const navLinkClass = ({ isActive }: { isActive: boolean }) =>
  isActive ? 'nav-item nav-item-active' : 'nav-item';

export default function Sidebar() {
  return (
    <aside className="sidebar">
      <div className="sidebar-brand">
        <span className="sidebar-brand-mark">✦</span>
        <span className="sidebar-brand-name">Glow Up</span>
      </div>

      <nav className="sidebar-nav">
        <NavLink to="/" end className={navLinkClass}>
          <span className="nav-icon">🏠</span>
          <span>Dashboard</span>
        </NavLink>

        <div className="sidebar-group-label">Self-Knowledge</div>
        {SECTION_CONFIGS.map((section) => (
          <NavLink
            key={section.type}
            to={`/sections/${section.type}`}
            className={navLinkClass}
          >
            <span className="nav-icon">{section.icon}</span>
            <span>{section.label}</span>
          </NavLink>
        ))}

        <div className="sidebar-group-label">Insights</div>
        <NavLink to="/ai-context" className={navLinkClass}>
          <span className="nav-icon">🤖</span>
          <span>AI Context Preview</span>
        </NavLink>
      </nav>
    </aside>
  );
}
