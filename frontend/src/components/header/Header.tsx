import { Link } from "react-router-dom";
import { Music2 } from "lucide-react";
import "./header.scss";
import { Divider } from "../divider/divider";

interface HeaderProps {
  isAuthenticated: boolean;
  username?: string;
  onLogout?: () => void;
}

export default function Header({ isAuthenticated, username, onLogout }: HeaderProps) {
  const publicLinks = [
    { to: "/", label: "Home" },
    { to: "/login", label: "Log in" },
    { to: "/register", label: "Sign Up" },
  ];

  const privateLinks = [
    { to: "/home", label: "Home" },
    { to: "/playlists", label: "Playlists" },
    { to: "/settings", label: "Settings" },
    { to: "/live-sessions", label: "Live Sessions" },
  ];

  const links = isAuthenticated ? privateLinks : publicLinks;

  return (
    <header className="header">
      {/* Left section: logo */}
      <div className="header__content">
        <div className="header__left">
          <Link to="/" className="header__logo">
            <Music2 size={28} className="header__logo-icon" />
            <span className="header__logo-text">MusicHub</span>
          </Link>
        </div>
        {/* Navigation links */}
        <nav className="header__nav">
          {links.map((link) => (
            <Link key={link.to} to={link.to} className="header__nav-link">
              {link.label}
            </Link>
          ))}
        </nav>
        {/* Right section: user info and logout */}
        {isAuthenticated && (
          <div className="header__right">
            <span className="header__username">{username}</span>
            <button className="header__logout-button" onClick={onLogout}>
              Logout
            </button>
          </div>
        )}
      </div>
      <Divider className="header__divider"/>
    

    </header>

  );
}
