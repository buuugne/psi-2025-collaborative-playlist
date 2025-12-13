import { useState, useEffect } from 'react';
import { Upload } from 'lucide-react';
import { UserService } from '../../../services/UserService';
import './settingsPage.scss';

const API_BASE = import.meta.env.VITE_API_URL;

interface User {
  id: number;
  username: string;
  profileImage?: string;
}

export default function Settings() {
  const [user, setUser] = useState<User | null>(null);
  const [imageFile, setImageFile] = useState<File | null>(null);
  const [preview, setPreview] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    loadUser();
  }, []);

  const loadUser = async () => {
    try {
      const data = await UserService.getCurrentUser();
      setUser(data);
    } catch (err) {
      console.error('Failed to load user:', err);
    }
  };

  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (file) {
      setImageFile(file);
      setPreview(URL.createObjectURL(file));
    }
  };

  const handleUpload = async () => {
    if (!imageFile || !user) return;
    
    setLoading(true);
    try {
      const updatedUser = await UserService.updateProfileImage(user.id, imageFile);
      setUser(updatedUser);
      setImageFile(null);
      setPreview(null);
      alert('Profile image updated!');
    } catch (err) {
      alert('Failed to update profile image');
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  const profileSrc = preview || (user?.profileImage 
    ? `${API_BASE}${user.profileImage}`
    : `https://api.dicebear.com/7.x/initials/svg?seed=${user?.username || 'User'}`);

  if (!user) {
    return <div className="settings-page__loading">Loading...</div>;
  }

  return (
    <div className="settings-page">
      <h1 className="settings-page__title">Settings</h1>
      
      <div className="settings-page__section">
        <h2 className="settings-page__section-title">Profile Picture</h2>
        
        <div className="settings-page__profile-container">
          <img
            src={profileSrc}
            alt="Profile"
            className="settings-page__profile-image"
          />
          
          <div className="settings-page__upload-container">
            <div className="settings-page__file-input-wrapper">
              <input
                id="profile-upload"
                type="file"
                accept="image/*"
                onChange={handleFileChange}
                className="settings-page__file-input"
              />
              <label htmlFor="profile-upload" className="settings-page__file-label">
                <Upload size={18} />
                Choose Image
              </label>
              {imageFile && (
                <p className="settings-page__file-name">
                  Selected: {imageFile.name}
                </p>
              )}
            </div>
            
            {imageFile && (
              <button
                onClick={handleUpload}
                disabled={loading}
                className="settings-page__upload-btn"
              >
                {loading ? 'Uploading...' : 'Upload Image'}
              </button>
            )}
          </div>
        </div>
        
        <div className="settings-page__info-grid">
          <div className="settings-page__info-item">
            <strong>Username:</strong>
            <span>{user.username}</span>
          </div>
        </div>
      </div>
    </div>
  );
}