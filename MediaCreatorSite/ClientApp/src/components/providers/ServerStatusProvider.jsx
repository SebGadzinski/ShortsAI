import React, { createContext, useState, useEffect } from "react";
import { _database } from "../../services/databaseService";

const ServerStatusContext = createContext();

const ServerStatusProvider = ({ children }) => {
  const [serverRunning, setServerRunning] = useState(true);
  const [lastServerRun, setLastServerRun] = useState("Unknown");

  const updateServerStatus = () => {
    _database.api("GET", "/home/ServerRunning").then((result) => {
      if (result) {
        console.log(result.data);
        setServerRunning(result.data.isRunning);
        setLastServerRun(result.data.lastCheck);
      }
    });
  };

  useEffect(() => {
    updateServerStatus();
  }, []);

  return (
    <ServerStatusContext.Provider
      value={{ serverRunning, lastServerRun, updateServerStatus }}
    >
      {children}{" "}
    </ServerStatusContext.Provider>
  );
};

export { ServerStatusContext, ServerStatusProvider };
