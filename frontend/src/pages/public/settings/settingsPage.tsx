import { useState, useEffect } from 'react';
import { Upload } from 'lucide-react';
import { UserService } from '../../../services/UserService';
import './settingsPage.scss';

<<<<<<< HEAD
=======
// For local dev with proxy, we can use relative URLs
// For production, use the full URL
>>>>>>> c1093bd (fixed settings issue)
const API_BASE = import.meta.env.VITE_API_URL || '';

interface User {
  id: number;
  username: string;
  role?: number;
  profileImage?: string;
}

export default function Settings() {
  const [user, setUser] = useState<User | null>(null);
  const [imageFile, setImageFile] = useState<File | null>(null);
  const [preview, setPreview] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);
  const [imageKey, setImageKey] = useState(0);

  useEffect(() => {
    loadUser();
  }, []);

  const loadUser = async () => {
    try {
      const data = await UserService.getCurrentUser();
      console.log('✅ Loaded user:', data);
      setUser(data);
    } catch (err) {
      console.error('❌ Failed to load user:', err);
    }
  };

  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (file) {
      console.log('📁 File selected:', {
        name: file.name,
        size: `${(file.size / 1024).toFixed(2)} KB`,
        type: file.type
      });
      setImageFile(file);
      setPreview(URL.createObjectURL(file));
    }
  };

  const handleUpload = async () => {
    if (!imageFile || !user) return;
    
    setLoading(true);
    try {
      console.log('📤 Starting upload...');
      const updatedUser = await UserService.updateProfileImage(user.id, imageFile);
      console.log('✅ Upload response:', updatedUser);
      
      setUser(updatedUser);
      setImageFile(null);
      
      if (preview) {
        URL.revokeObjectURL(preview);
        setPreview(null);
      }
      
      setImageKey(prev => prev + 1);
      alert('Profile image updated successfully!');
      
    } catch (err: any) {
<<<<<<< HEAD
      alert(err?.response?.data || 'Failed to update profile image');
      console.error(err);
=======
      console.error('❌ Upload error:', err);
      alert(err?.response?.data || 'Failed to update profile image');
>>>>>>> c1093bd (fixed settings issue)
    } finally {
      setLoading(false);
    }
  };

  const getProfileImageUrl = () => {
<<<<<<< HEAD
    if (preview) return preview;
    if (user?.profileImage) return `${API_BASE}${user.profileImage}?t=${imageKey}`;
=======
    if (preview) {
      return preview;
    }
    
    if (user?.profileImage) {
      // In production with VITE_API_URL set: https://musichub-qwoh.onrender.com/profiles/image.png
      // In development with proxy: /profiles/image.png (proxied to localhost:5000)
      const imageUrl = `${API_BASE}${user.profileImage}?t=${imageKey}`;
      console.log('🖼️ Image URL:', imageUrl);
      return imageUrl;
    }
    
>>>>>>> c1093bd (fixed settings issue)
    return `https://api.dicebear.com/7.x/initials/svg?seed=${user?.username || 'User'}`;
  };

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
            key={imageKey}
            src={getProfileImageUrl()}
            alt="Profile"
            className="settings-page__profile-image"
<<<<<<< HEAD
            onError={(e) => {
=======
            onLoad={() => console.log('✅ Image loaded successfully')}
            onError={(e) => {
              console.error('❌ Image failed to load:', getProfileImageUrl());
>>>>>>> c1093bd (fixed settings issue)
              e.currentTarget.src = `https://api.dicebear.com/7.x/initials/svg?seed=${user?.username || 'User'}`;
            }}
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