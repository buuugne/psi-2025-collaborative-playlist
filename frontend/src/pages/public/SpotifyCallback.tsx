import { useEffect, useState } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { authService } from '../../services/authService';

export default function SpotifyCallback() {
  const [searchParams] = useSearchParams();
  const navigate = useNavigate();
  const [status, setStatus] = useState('Processing...');

  useEffect(() => {
    const code = searchParams.get('code');
    const error = searchParams.get('error');

    if (error) {
      console.error('Spotify auth error:', error);
      setStatus('Spotify authorization was denied or failed.');
      setTimeout(() => navigate('/settings'), 2000);
      return;
    }

    if (code) {
      // Send code to your backend
      fetch('https://musichub-qwoh.onrender.com/', {
        method: 'POST',
        headers: { 
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${authService.getToken()}` // If you need auth
        },
        body: JSON.stringify({ code })
      })
        .then(res => {
          if (!res.ok) throw new Error('Token exchange failed');
          return res.json();
        })
        .then(data => {
          // Save tokens to localStorage
          localStorage.setItem('spotify_access_token', data.accessToken);
          if (data.refreshToken) {
            localStorage.setItem('spotify_refresh_token', data.refreshToken);
          }
          
          setStatus('Successfully connected to Spotify!');
          setTimeout(() => navigate('/settings'), 1500);
        })
        .catch(err => {
          console.error('Token exchange failed:', err);
          setStatus('Failed to connect to Spotify. Please try again.');
          setTimeout(() => navigate('/settings'), 2000);
        });
    } else {
      setStatus('No authorization code received.');
      setTimeout(() => navigate('/settings'), 2000);
    }
  }, [searchParams, navigate]);

  return (
    <div style={{ 
      display: 'flex', 
      justifyContent: 'center', 
      alignItems: 'center', 
      height: '100vh',
      flexDirection: 'column',
      gap: '20px'
    }}>
      <div className="spinner" />
      <h2>{status}</h2>
    </div>
  );
}