import { useState, useEffect } from 'react';
import { useStore } from './main';

type Commands =
  | {
      type: 'sas';
      data: {
        enable: boolean;
      };
    }
  | {
      type: 'watch';
      data: string[];
    }
  | {
      type: 'throttle';
      data: number;
    };

/**
 * Websocket connection to the KSP websocket server
 *
 * Should return current connection status and a function to send commands
 */
export const useSocket = () => {
  const { connect, connected, websocket } = useStore();

  useEffect(() => {
    if (connected === 'disconnected')
      connect((ws) => {
        ws.send(
          JSON.stringify({
            type: 'watch',
            data: ['altitude', 'sas', 'throttle'],
          })
        );
      });
  }, []);

  const sendCommand = (command: Commands) => {
    if (websocket) {
      websocket.send(JSON.stringify(command));
    } else {
      console.error('No websocket connection');
    }
  };

  return {
    sendCommand,
    connected,
  };
};
