import { Routes, Route, Navigate, useNavigate } from "react-router-dom";

import MainLayout from "../layouts/MainLayout";

import LandingPage from "../pages/public/landing/landingPage";
import LoginPage from "../pages/public/login/loginPage";
import RegisterPage from "../pages/public/register/registerPage";
import ProtectedRoute from "../components/ProtectedRoute";
import { authService } from "../services/authService";
import Settings from '../pages/public/settings/settingsPage';
import HomePage from "../pages/protected/home/HomePage";
import PlaylistDetailPage from "../pages/protected/home/PlaylistDetailPage";
import PlaylistsPage from "../pages/protected/playlistsPage/playlistsPage";
import LiveSessionsPage from "../pages/protected/LiveSessions/LiveSessionsPage";

export default function AppRoutes() {
  const navigate = useNavigate();
  const isAuthenticated = authService.isAuthenticated();
  const user = authService.getUser(); 

  const handleLogout = () => {
    authService.logout();
    navigate("/login"); 
  };

  return (
    <Routes>
  
      
      <Route element={<MainLayout isAuthenticated={isAuthenticated} username={user?.username} onLogout={handleLogout} />}>

        {/* Public routes */}
        <Route
        path="/"
        element={isAuthenticated ? <Navigate to="/home" /> : <LandingPage />}
        />

        <Route
          path="/login"
          element={isAuthenticated ? <Navigate to="/home" /> : <LoginPage />}
        />

        <Route
          path="/register"
          element={isAuthenticated ? <Navigate to="/home" /> : <RegisterPage />}
        />

        {/* Protected routes */}
        <Route
          path="/home"
          element={
            <ProtectedRoute>
              <HomePage />
            </ProtectedRoute>
          }
        />
        
        <Route
          path="/live-sessions"
          element={
            <ProtectedRoute>
              <LiveSessionsPage />
            </ProtectedRoute>
          }
        />
        
        <Route
          path="/playlists"
          element={
            <ProtectedRoute>
              <PlaylistsPage />
            </ProtectedRoute>
          }
        />

        <Route
          path="/playlist/:id"
          element={
            <ProtectedRoute>
              <PlaylistDetailPage />
            </ProtectedRoute>
          }
        />

        <Route
          path="/settings"
          element={
            <ProtectedRoute>
              <Settings />
            </ProtectedRoute>
          }
        />

      </Route>
    </Routes>
  );
}