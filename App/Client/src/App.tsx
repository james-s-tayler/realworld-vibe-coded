import { BrowserRouter, Routes, Route } from 'react-router';
import { lazy, Suspense } from 'react';
import { Content, Loading } from '@carbon/react';
import { AuthProvider } from './context/AuthContext';
import { FeatureFlagProvider } from './context/FeatureFlagContext';
import { ToastProvider } from './context/ToastContext';
import { AppHeader } from './components/AppHeader';
import { ToastContainer } from './components/ToastContainer';
import { ProtectedRoute } from './components/ProtectedRoute';
import { RoleProtectedRoute } from './components/RoleProtectedRoute';

const HomePage = lazy(() => import('./pages/HomePage').then(m => ({ default: m.HomePage })));
const LoginPage = lazy(() => import('./pages/LoginPage').then(m => ({ default: m.LoginPage })));
const RegisterPage = lazy(() => import('./pages/RegisterPage').then(m => ({ default: m.RegisterPage })));
const ProfilePage = lazy(() => import('./pages/ProfilePage').then(m => ({ default: m.ProfilePage })));
const ArticlePage = lazy(() => import('./pages/ArticlePage').then(m => ({ default: m.ArticlePage })));
const EditorPage = lazy(() => import('./pages/EditorPage').then(m => ({ default: m.EditorPage })));
const SettingsPage = lazy(() => import('./pages/SettingsPage').then(m => ({ default: m.SettingsPage })));
const UsersPage = lazy(() => import('./pages/UsersPage').then(m => ({ default: m.UsersPage })));
const ForbiddenPage = lazy(() => import('./pages/ForbiddenPage').then(m => ({ default: m.ForbiddenPage })));

const LazyFallback = () => (
  <div className="loading-fullscreen">
    <Loading description="Loading..." withOverlay={false} />
  </div>
);

function App() {
  return (
    <BrowserRouter>
      <AuthProvider>
        <FeatureFlagProvider>
          <ToastProvider>
          <AppHeader />
          <Content>
          <ToastContainer />
          <Suspense fallback={<LazyFallback />}>
            <Routes>
              <Route path="/login" element={<LoginPage />} />
              <Route path="/register" element={<RegisterPage />} />
              <Route path="/forbidden" element={<ForbiddenPage />} />
              <Route
                path="/"
                element={
                  <ProtectedRoute>
                    <HomePage />
                  </ProtectedRoute>
                }
              />
              <Route
                path="/article/:slug"
                element={
                  <ProtectedRoute>
                    <ArticlePage />
                  </ProtectedRoute>
                }
              />
              <Route
                path="/editor"
                element={
                  <ProtectedRoute>
                    <EditorPage />
                  </ProtectedRoute>
                }
              />
              <Route
                path="/editor/:slug"
                element={
                  <ProtectedRoute>
                    <EditorPage />
                  </ProtectedRoute>
                }
              />
              <Route
                path="/profile/:username"
                element={
                  <ProtectedRoute>
                    <ProfilePage />
                  </ProtectedRoute>
                }
              />
              <Route
                path="/settings"
                element={
                  <ProtectedRoute>
                    <SettingsPage />
                  </ProtectedRoute>
                }
              />
              <Route
                path="/users"
                element={
                  <RoleProtectedRoute requiredRoles={['ADMIN']}>
                    <UsersPage />
                  </RoleProtectedRoute>
                }
              />
            </Routes>
          </Suspense>
          </Content>
        </ToastProvider>
        </FeatureFlagProvider>
      </AuthProvider>
    </BrowserRouter>
  );
}

export default App
