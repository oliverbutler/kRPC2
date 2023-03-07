import React from 'react';
import ReactDOM from 'react-dom/client';
import App from './App';
import './index.css';
import { create } from 'zustand';

interface Watches {
  altitude: number | undefined;
  sas: boolean | undefined;
  throttle: number | undefined;
}

interface State {
  watch: Watches;
  websocket: WebSocket | null;
  connect: (onConnect: (ws: WebSocket) => void) => void;
  connected: 'connecting' | 'connected' | 'disconnected';
}

export const useStore = create<State>((set) => ({
  watch: {
    altitude: undefined,
    sas: undefined,
    throttle: undefined,
  },
  websocket: null,
  connect: (onConnect: (ws: WebSocket) => void) => {
    const websocket = new WebSocket('ws://10.0.0.165:6674/ws');

    set({ connected: 'connecting' });

    websocket.onopen = function (event) {
      set({ connected: 'connected' });

      onConnect(websocket);
    };

    websocket.onclose = function (event) {
      set({ connected: 'disconnected' });
    };

    websocket.onerror = function (event) {
      set({ connected: 'disconnected' });
    };

    websocket.onmessage = function (event) {
      const data = JSON.parse(event.data);

      if (data.type === 'altitude') {
        set((state) => {
          return {
            watch: {
              ...state.watch,
              altitude: data.data,
            },
          };
        });
      } else if (data.type === 'sas') {
        set((state) => {
          return {
            watch: {
              ...state.watch,
              sas: data.data,
            },
          };
        });
      } else if (data.type === 'throttle') {
        set((state) => {
          return {
            watch: {
              ...state.watch,
              throttle: data.data,
            },
          };
        });
      }
    };

    set({ websocket });
  },
  connected: 'disconnected',
}));

ReactDOM.createRoot(document.getElementById('root') as HTMLElement).render(
  <React.StrictMode>
    <App />
  </React.StrictMode>
);
