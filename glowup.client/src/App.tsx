import { Routes, Route, Link } from 'react-router-dom';
import AppLayout from './layout/AppLayout';
import DashboardPage from './pages/DashboardPage';
import SectionPage from './pages/SectionPage';
import AiContextPreviewPage from './pages/AiContextPreviewPage';
import AiChatPage from './pages/AiChatPage';
import CustomSectionPage from './pages/CustomSectionPage';

function NotFound() {
  return (
    <div className="page">
      <div className="page-head">
        <h2 className="page-title">Page not found</h2>
        <p className="page-subtitle">
          That page doesn’t exist. <Link to="/">Back to dashboard</Link>.
        </p>
      </div>
    </div>
  );
}

export default function App() {
  return (
    <Routes>
      <Route element={<AppLayout />}>
        <Route index element={<DashboardPage />} />
        <Route path="sections/:sectionType" element={<SectionPage />} />
        <Route path="custom-sections/new" element={<CustomSectionPage />} />
        <Route path="custom-sections/:id" element={<CustomSectionPage />} />
        <Route path="ai-context" element={<AiContextPreviewPage />} />
        <Route path="ai-chat" element={<AiChatPage />} />
        <Route path="*" element={<NotFound />} />
      </Route>
    </Routes>
  );
}
