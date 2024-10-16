import React, { useEffect, useState } from "react";
import { Route, Routes } from "react-router-dom";
import AppRoutes from "./AppRoutes";
import Layout from "./components/shared/Layout";
import { ThemeProvider } from "@mui/material";
import themes from "./themes";
import localStorageService from "./services/localStorageService";
import "./custom.css";
import { SessionProvider } from "./components/providers/SessionProvider"; // Import SessionProvider instead of SessionContext
import { CssBaseline } from "@mui/material";
import { ToastContainer } from "react-toastify";
import "react-toastify/dist/ReactToastify.css";
import { BaseModalProvider } from "./components/providers/BaseModalProvider";
import { ServerStatusProvider } from "./components/providers/ServerStatusProvider";

const App = () => {
  const [theme, setTheme] = React.useState(
    localStorageService.getItem("theme") ?? "light"
  );
  const [showAuthModal, setShowAuthModal] = useState(false);

  useEffect(() => {
    localStorageService.setItem("theme", theme);
  }, [theme]);

  const selectTheme = (selectedTheme) => {
    if (themes[selectedTheme]) setTheme(selectedTheme);
  };

  const handleAuthModalOpen = () => {
    setShowAuthModal(true);
  };

  const handleAuthModalClose = () => {
    setShowAuthModal(false);
  };

  return (
    <SessionProvider>
      <BaseModalProvider>
        <ThemeProvider theme={themes[theme]}>
          <ServerStatusProvider>
            <CssBaseline />
            <Layout
              showAuthModal={showAuthModal}
              handleAuthModalClose={handleAuthModalClose}
              handleAuthModalOpen={handleAuthModalOpen}
              themes={themes}
              selectTheme={selectTheme}
            >
              <Routes>
                {" "}
                {AppRoutes.map((route, index) => {
                  const { element, ...rest } = route;
                  return (
                    <Route
                      key={index}
                      {...rest}
                      element={element({
                        handleAuthModalClose,
                        handleAuthModalOpen
                      })}
                    />
                  );
                })}{" "}
              </Routes>
            </Layout>
            <ToastContainer
              position="top-center"
              autoClose={5000}
              hideProgressBar={false}
              newestOnTop={false}
              closeOnClick
              rtl={false}
              pauseOnFocusLoss
              draggable
              pauseOnHover
              theme={theme.includes("dark") ? "dark" : "light"}
            />
          </ServerStatusProvider>
        </ThemeProvider>
      </BaseModalProvider>
    </SessionProvider>
  );
};

export default App;
