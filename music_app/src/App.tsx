import { useState } from 'react'
import './App.css'

function App() {
  const [isPopupOpen, setPopupOpen] = useState(false);

  return (
    <div className="flex flex-col items-center justify-center h-screen text-center"> {/* flex is for centering */}
      <h1 className="text-4xl font-bold mb-4">Playlists :</h1>
      <MyButton onClick={() => setPopupOpen(true)} />
      {isPopupOpen && (
        <MyPopup onClose={() => setPopupOpen(false)} />
      )}
    </div>
  );
}


function MyButton({ onClick }: { onClick: () => void }) {
  return (
    <button 
      onClick={onClick}
      className="bg-blue-500 hover:bg-blue-600 text-white px-4 py-2 rounded-lg font-semibold mt-4"
    >
      +
    </button>
  );
}

function MyPopup({ onClose }: { onClose: () => void }) {
  return (
    <div className="popup-overlay">
      <div className="popup-content">
        <h2>Popup content</h2>
        <button onClick={onClose}>
          Close
        </button>
      </div>
    </div>
  );
}

export default App;