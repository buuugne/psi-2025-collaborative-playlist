import "bootstrap/dist/css/bootstrap.min.css";
import "./index.css";
import "./styles/global.scss";
import "./styles/App.css";
import { BrowserRouter as Router } from "react-router-dom";
import AppRoutes from "./routes/AppRoutes";
import { SpotifyPlayerProvider } from "./context/SpotifyPlayerContext";
import SpotifyPlayerBar from "./components/SpotifyPlayerBar";
import { useEffect } from "react";

function App() {
  useEffect(() => {
    const token =
      "BQAeo-oL6Ktg-r7ydu2t-zDD59yEBY-NltBIZmU3_-tHuJ3lHYO7HJ1TJ78pzAxcPpHUTuYrADrXxBXcq-tts_pRvRaCdEYwDz_hQdwtfz_8ypTureKuzI817EDrc5X2uKuTYOfAZCW1glKuHsMWK2s5JTR7KKz4NKvw8fRPJ5rWQoMEQA4aVCkjiRLJFjY9-LmueW7SUi4oUP5JhvsXT-3d0VK8m9uplxn659UqebYPyb-gzTO8";

    if (!token) {
      console.error(
        "NO SPOTIFY TOKEN PROVIDED — The player will NOT work."
      );
    }

   
  }, []);

  return (
    <SpotifyPlayerProvider>
      <Router>
        <AppRoutes />
      </Router>
      <SpotifyPlayerBar />
    </SpotifyPlayerProvider>
  );
}

export default App;
