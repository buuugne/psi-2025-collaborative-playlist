import { Outlet } from "react-router-dom";
import Header from "../components/header/Header";
import SpotifyPlayerBar from "../components/SpotifyPlayerBar";
import { authService } from "../services/authService";

interface MainLayoutProps {
  isAuthenticated: boolean;
  username?: string;
  onLogout: () => void;
}

const MainLayout: React.FC<MainLayoutProps> = ({ isAuthenticated, username, onLogout }) => {
  return (
    <div>
      <Header isAuthenticated={isAuthenticated} username={username} onLogout={onLogout} />
      <main>
        <Outlet />
      </main>
      {/* Only show player bar on protected routes */}
      {isAuthenticated && <SpotifyPlayerBar />}
    </div>
  );
};

export default MainLayout;