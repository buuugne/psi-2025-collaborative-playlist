import { useState, useEffect } from "react";
import './styles/App.css';
import PlaylistList from './components/Playlist';
import 'bootstrap/dist/css/bootstrap.min.css';
import { Button } from 'react-bootstrap';
import SearchSongModal from './components/SearchSongModal';
import PlaylistModal from './components/PlaylistModal';
import { usePlaylists } from "./components/UsePlaylists";

function App() {
  const [isSearchOpen, setSearchOpen] = useState(false);
  const [showModal, setShowModal] = useState(false);

  const { playlists, addPlaylist, deletePlaylist, updatePlaylist } = usePlaylists();

  return (
    <div className="main-page">
      <aside className="left-side">
        <PlaylistList
          playlists={playlists}
          deletePlaylist={deletePlaylist}
          updatePlaylist={updatePlaylist}
        />
      </aside>

      <div className="vDivider"></div>

      <div className="right-side">
        <input type="text" name="search-bar" id="search-bar" placeholder="Mock search bar" />
        <div className="home-page-btns">
          <div className="buttons-up">
            <div>
              <Button variant="primary" onClick={() => setSearchOpen(true)}>
                Add a song
              </Button>
              <SearchSongModal show={isSearchOpen} onHide={() => setSearchOpen(false)} />
            </div>
          </div>

          <div className="buttons-down">
            <button className="add-playlist-btn" onClick={() => setShowModal(true)}>
              + New Playlist
            </button>
            {showModal && (
              <PlaylistModal
                close={() => setShowModal(false)}
                onPlaylistCreated={addPlaylist}
              />
            )}
          </div>
        </div>
      </div>
    </div>
  );
}

export default App;
