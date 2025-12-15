import {Sparkles, Plus, Play, UsersRound, Search, MessageCircle} from 'lucide-react';
import "./landingPage.scss"
import { useNavigate } from "react-router-dom";

export default function LandingPage() {
  const navigate = useNavigate();

    return (
      <div className="landing-page">
        <div className="landing-page__hero-badge">
          <Sparkles size={18} strokeWidth={2} className="landing-page__icon"/>
          Collaborative playlists
        </div>
        <h1 className="landing-page__header">Your Music, Together. Instantly.</h1>
        <h3 className="landing-page__desc">Create, share, and collaborate on playlists with friends. Search songs, drag to add, and listen as a group.</h3>
        <div className="landing-page__buttons">
          <button 
            className="landing-page__button landing-page__button--primary" 
            type="button"
            onClick={() => navigate("/register")}>
            <Play strokeWidth={2} size={18} className="landing-page__icon"/>
            Get started
          </button>
        </div>
        <div className="landing-page__perks">
          <div className="landing-page__perk">
            <UsersRound size={24} className="landing-page__perk-icon"/>
            <h3 className="landing-page__perk-title">Invite with one link</h3>
            <p className="landing-page__perk-desc">Share and start adding tracks in seconds.</p>
          </div>
          <div className="landing-page__perk">
            <MessageCircle size={24} className="landing-page__perk-icon"/>
            <h3 className="landing-page__perk-title">Reactions</h3>
            <p className="landing-page__perk-desc">Drop emojis and comments on tracks.</p>
          </div>
          <div className="landing-page__perk">
            <Search size={24} className="landing-page__perk-icon"/>
            <h3 className="landing-page__perk-title">Powerful Search</h3>
            <p className="landing-page__perk-desc">Find songs fast across genres and moods.</p>
          </div>
        </div>
      </div>
    );
  }