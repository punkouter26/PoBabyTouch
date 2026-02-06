import { Routes, Route } from 'react-router-dom';
import Home from './pages/Home.tsx';
import Game from './pages/Game.tsx';
import Leader from './pages/Leader.tsx';
import Stats from './pages/Stats.tsx';
import Diag from './pages/Diag.tsx';

export default function App() {
  return (
    <Routes>
      <Route path="/" element={<Home />} />
      <Route path="/game" element={<Game />} />
      <Route path="/leader" element={<Leader />} />
      <Route path="/stats" element={<Stats />} />
      <Route path="/diag" element={<Diag />} />
    </Routes>
  );
}
