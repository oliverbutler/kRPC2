import { useState } from 'react';
import './App.css';
import { useSocket } from './useSocket';
import { useStore } from './main';

function App() {
  const { connected, sendCommand } = useSocket();

  const altitude = useStore((s) => s.watch.altitude);
  const sas = useStore((s) => s.watch.sas);
  const throttle = useStore((s) => s.watch.throttle);

  const handleToggleSas = () => {
    sendCommand({ type: 'sas', data: { enable: !sas } });
  };

  return (
    <div className="App">
      <h1>KSP</h1>
      <p>{connected}</p>
      {<p>Altitude: {altitude?.toFixed(0)}m</p>}
      {<p>SAS: {sas ? 'Enabled' : 'Disabled'}</p>}
      {<p>Throttle: {throttle?.toFixed(0)}%</p>}
      <button onClick={handleToggleSas}>
        {sas ? 'Disable SAS' : 'Enable SAS'}
      </button>

      <div className="throttle">
        <button onClick={() => sendCommand({ type: 'throttle', data: 0 })}>
          0%
        </button>
        <button onClick={() => sendCommand({ type: 'throttle', data: 0.25 })}>
          25%
        </button>
        <button onClick={() => sendCommand({ type: 'throttle', data: 0.5 })}>
          50%
        </button>
        <button onClick={() => sendCommand({ type: 'throttle', data: 0.75 })}>
          75%
        </button>
        <button onClick={() => sendCommand({ type: 'throttle', data: 1 })}>
          100%
        </button>
      </div>
    </div>
  );
}

export default App;
