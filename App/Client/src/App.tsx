import { BrowserRouter, Routes, Route } from 'react-router';
import { AuthProvider } from './context/AuthContext';
import { FeatureFlagProvider } from './context/FeatureFlagContext';
import { ProtectedRoute } from './components/ProtectedRoute';
import { RoleProtectedRoute } from './components/RoleProtectedRoute';
import { AppHeader } from './components/AppHeader';
import { AppSidebar } from './components/AppSidebar';
import { DashboardPage } from './pages/DashboardPage';
import { LoginPage } from './pages/LoginPage';
import { RegisterPage } from './pages/RegisterPage';
import { ProfilePage } from './pages/ProfilePage';
import { SettingsPage } from './pages/SettingsPage';
import { UsersPage } from './pages/UsersPage';
import { ForbiddenPage } from './pages/ForbiddenPage';

function App() {
  return (
    <BrowserRouter>
      <AuthProvider>
        <FeatureFlagProvider>
        <AppHeader />
        <AppSidebar />
        <Routes>
          <Route
            path="/"
            element={
              <ProtectedRoute>
                <DashboardPage />
              </ProtectedRoute>
            }
          />
          <Route path="/login" element={<LoginPage />} />
          <Route path="/register" element={<RegisterPage />} />
          <Route
            path="/profile/:username"
            element={
              <ProtectedRoute>
                <ProfilePage />
              </ProtectedRoute>
            }
          />
          <Route
            path="/editor"
            element={
              <ProtectedRoute>
                <div />
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
          <Route path="/forbidden" element={<ForbiddenPage />} />
        </Routes>
        </FeatureFlagProvider>
      </AuthProvider>
    </BrowserRouter>
  );
}

export default App
