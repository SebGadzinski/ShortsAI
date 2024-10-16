import React, { createContext, useState, useEffect } from "react";
import { _database } from "../../services/databaseService";
import { _notify } from "../../services/notifyService";

const SessionContext = createContext();

const SessionProvider = ({ children }) => {
  const [sessionData, setSessionData] = useState(null);

  const updateSessionData = (data) => {
    setSessionData(data);
  };

  const isLoggedIn = () => {
    return sessionData?.user?.id;
  };

  const logout = () => {
    _database.api("POST", "/auth/Logout").then((result) => {
      if (result) {
        if (result.status === 1) {
          updateSessionData(result.data);
          window.location = "/";
        } else {
          _notify.error(result.errorResult);
        }
      } else {
        _notify.error(
          "A unexpected error occurred, check your network connection."
        );
      }
    });
  };

  useEffect(() => {
    _database.api_form_data("POST", "/auth/GetSession").then((result) => {
      if (result) {
        if (result.status === 1) {
          updateSessionData(result.data);
        } else {
          _notify.error(result.errorResult);
        }
      } else {
        _notify.error(
          "Could not get session. A unexpected error occurred, check your network connection."
        );
      }
    });
  }, []);

  return (
    <SessionContext.Provider
      value={{ sessionData, updateSessionData, isLoggedIn, logout }}
    >
      {children}{" "}
    </SessionContext.Provider>
  );
};

export { SessionContext, SessionProvider };
