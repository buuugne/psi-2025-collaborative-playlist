import { useState } from "react";
import Form from "react-bootstrap/Form";
import InputGroup from "react-bootstrap/InputGroup";
import Button from "react-bootstrap/Button";
import { type Track, type Song, type SpotifyResponse } from "./Spotify";

interface SearchBarProps {
  onSongSelect: (song: Song) => void;
}

export default function SearchBar({ onSongSelect }: SearchBarProps) {
  const [query, setQuery] = useState("");
  const [results, setResults] = useState<Track[]>([]);

  const handleSearch = async () => {
  if (!query.trim()) return;

  try {
    const response = await fetch(
      `http://localhost:5000/api/Spotify/search/${encodeURIComponent(query)}`
    );

    // ✅ If backend returned an error (500 etc.)
    if (!response.ok) {
      const text = await response.text(); // read the error text
      throw new Error(`Server error ${response.status}: ${text}`);
    }

    // ✅ Try parsing JSON only when response is OK
    const data: SpotifyResponse = await response.json();
    setResults(data.tracks?.items || []);
  } catch (error) {
    console.error("Search failed:", error);
    alert("Spotify search failed — check backend or token validity.");
    setResults([]); // clear results on error
  }
};


  return (
    <>
      <InputGroup className="mb-3">
        <InputGroup.Text id="inputGroup-sizing-default">🔍</InputGroup.Text>
        <Form.Control
          value={query}
          onChange={(e) => setQuery(e.target.value)}
          onKeyDown={(e) => e.key === "Enter" && handleSearch()}
          placeholder="Search for songs..."
        />
        <Button variant="success" onClick={handleSearch}>
          Search
        </Button>
      </InputGroup>

      <ul>
        {results.map((track) => (
          <li key={track.id}>
            <strong>{track.name}</strong> by{" "}
            {track.artists.map((a) => a.name).join(", ")} <br />
            Album: {track.album.name} <br />
            <a
              href={track.external_urls.spotify}
              target="_blank"
              rel="noopener noreferrer"
            >
              Listen on Spotify
            </a>{" "}
            <br />
            <button
              className="bg-green-500 text-white px-3 py-1 rounded"
              onClick={() =>
                onSongSelect({
                  Title: track.name,
                  Artist: track.artists.map((a) => a.name).join(", "),
                  Album: track.album.name,
                  Url: track.external_urls.spotify,
                })
              }
            >
              +
            </button>
          </li>
        ))}
      </ul>
      <br />
    </>
  );
}
