import "bootstrap/dist/css/bootstrap.min.css";
import "./index.css";
import "./styles/global.scss";
import "./styles/App.css";

import { BrowserRouter as Router } from "react-router-dom";
import AppRoutes from "./routes/AppRoutes";
import { SpotifyPlayerProvider } from "./context/SpotifyPlayerContext";
import SpotifyPlayerBar from "./components/SpotifyPlayerBar";
import SpotifyLogin from "./components/SpotifyLogin";

function App() {
  return (
    <SpotifyPlayerProvider>
      <SpotifyLogin />
      <Router>
        <AppRoutes />
      </Router>
      <SpotifyPlayerBar />
    </SpotifyPlayerProvider>
  );
}

export default App;