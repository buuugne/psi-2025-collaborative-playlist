import { useState } from 'react';
import Form from 'react-bootstrap/Form';
import InputGroup from 'react-bootstrap/InputGroup';
import Button from 'react-bootstrap/Button';
import { type Track, type SpotifyResponse } from './Spotify';

export default function SearchBar() {
  const [query, setQuery] = useState('');
  const [results, setResults] = useState<Track[]>([]);

  const handleSearch = async () => {
    if (!query.trim()) return;

    try {
      // ✅ Use environment variable instead of hardcoded URL
      const apiBaseUrl = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000';
      const response = await fetch(
        `${apiBaseUrl}/api/Spotify/search/${encodeURIComponent(query)}`
      );
      const data: SpotifyResponse = await response.json();
      setResults(data.tracks?.items || []);
    } catch (error) {
      console.error('Search failed:', error);
      setResults([]);
    }
  };

  return (
    <>
      <InputGroup className="mb-3">
        <InputGroup.Text id="inputGroup-sizing-default">🔍</InputGroup.Text>
        <Form.Control
          value={query}
          onChange={(e) => setQuery(e.target.value)}
          onKeyDown={(e) => e.key === 'Enter' && handleSearch()}
          placeholder="Search for songs..."
        />
        <Button variant="success" onClick={handleSearch}>
          Search
        </Button>
      </InputGroup>

      <ul>
        {results.map((track) => (
          <li key={track.id}>
            <strong>{track.name}</strong> by{' '}
            {track.artists.map((a) => a.name).join(', ')} <br />
            Album: {track.album.name} <br />
            <a href={track.external_urls.spotify} target="_blank" rel="noopener noreferrer">
              Listen on Spotify
            </a>
          </li>
        ))}
      </ul>
      <br />
    </>
  );
}