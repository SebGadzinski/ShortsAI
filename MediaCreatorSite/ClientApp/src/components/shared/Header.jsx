import React, { useContext, useEffect, useState } from "react";
import { makeStyles } from "@mui/styles";
import { SessionContext } from "../providers/SessionProvider";
import {
  AppBar,
  Toolbar,
  IconButton,
  Button,
  Menu,
  MenuItem,
  Typography,
  Hidden
} from "@mui/material";
import { _database } from "../../services/databaseService";
import { Link } from "react-router-dom";
import StatusBall from "./StatusBall";
import { ServerStatusContext } from "../providers/ServerStatusProvider";

const useStyles = makeStyles((theme) => ({
  root: {
    flexGrow: 1
  },
  menuButton: {
    marginRight: theme.spacing(2)
  },
  title: {
    flexGrow: 1
  },
  barButton: {
    margin: "auto 10px !important",
    [theme.breakpoints.down("lg")]: {
      margin: "auto 5px !important" // Font size for small screens and below
    }
  },
  typography: {
    margin: "auto 10px 0px 10px !important",
    padding: "none",
    fontSize: "3rem !important", // Default font size
    [theme.breakpoints.down("lg")]: {
      fontSize: "0px !important" // Font size for small screens and below
    },
    textAlign: "center"
  }
}));

export default function Header({
  handleAuthModalOpen,
  themes,
  selectTheme,
  logo
}) {
  const classes = useStyles();
  const [credits, setCredits] = useState(-1);
  const [anchorEl, setAnchorEl] = useState(null);
  const [mobileAnchorEl, setMobileAnchorEl] = useState(null);
  const { isLoggedIn, logout } = useContext(SessionContext);
  const { serverRunning, lastServerRun } = useContext(ServerStatusContext);

  const handleThemeMenuOpen = (event) => {
    setAnchorEl(event.currentTarget);
  };

  const handleThemeMenuClose = () => {
    setAnchorEl(null);
  };

  const handleThemeChange = (theme) => {
    selectTheme(theme);
    handleThemeMenuClose();
  };

  const handleMobileMenuOpen = (event) => {
    setMobileAnchorEl(event.currentTarget);
  };

  const handleMobileMenuClose = () => {
    setMobileAnchorEl(null);
  };

  const menuItems = isLoggedIn()
    ? [
        {
          link: "/my-profile?purchase=1",
          text: `${credits} C`,
          onClick: handleMobileMenuClose
        },
        { link: "/all-videos", text: "Videos", onClick: handleMobileMenuClose },
        {
          link: "/my-profile",
          text: "Profile",
          onClick: handleMobileMenuClose
        },
        {
          onClick: () => {
            logout();
            handleMobileMenuClose();
          },
          text: "Logout"
        }
      ]
    : [{ onClick: handleAuthModalOpen, text: "Login" }];

  const updateCredits = () => {
    _database.api("GET", "/profile/GetCredits").then((result) => {
      if (result) {
        if (result.status === 1) {
          setCredits(result.data);
        }
      }
    });
  };

  useEffect(() => {
    //Get credits
    updateCredits();
  }, []);

  return (
    <div className={classes.root}>
      <AppBar position="static">
        <Toolbar>
          <IconButton
            edge="start"
            className={classes.menuButton}
            color="inherit"
            aria-label="logo"
            href="/"
          >
            <img src={logo} alt="logo" />
            <Typography component="span" className={classes.typography}>
              Shorts AI®
            </Typography>
          </IconButton>
          <div className={classes.title} />
          {/* Desktop */}
          <Hidden lgDown>
            {" "}
            {/* Use this wrapper to hide on small screens */}
            {isLoggedIn() ? (
              <>
                <Button
                  className={classes.barButton}
                  color="inherit"
                  href="/my-profile?purchase=1"
                >
                  {credits >= 0 ? credits : "0"} C
                </Button>
                <Button
                  className={classes.barButton}
                  color="inherit"
                  href="/all-videos"
                >
                  Videos
                </Button>
                <Button
                  className={classes.barButton}
                  color="inherit"
                  href="/my-profile"
                >
                  My Profile
                </Button>
                <Button
                  className={classes.barButton}
                  color="inherit"
                  onClick={logout}
                >
                  Logout
                </Button>
              </>
            ) : (
              <Button
                onClick={handleAuthModalOpen}
                className={classes.barButton}
                color="inherit"
              >
                Login
              </Button>
            )}
          </Hidden>
          {/* Mobile */}
          <Hidden lgUp>
            <Button
              color="inherit"
              className={classes.barButton}
              onClick={handleMobileMenuOpen}
              aria-controls="mobile-menu"
              aria-haspopup="true"
            >
              Menu
            </Button>

            <Menu
              id="mobile-menu"
              anchorEl={mobileAnchorEl}
              keepMounted
              open={Boolean(mobileAnchorEl)}
              onClose={handleMobileMenuClose}
            >
              {menuItems.map((item, index) => (
                <MenuItem
                  key={index}
                  component={item.link ? Link : "button"}
                  to={item.link}
                  onClick={item.onClick}
                  color="inherit"
                >
                  {item.text}
                </MenuItem>
              ))}
            </Menu>
          </Hidden>

          <Button
            color="inherit"
            className={classes.barButton}
            onClick={handleThemeMenuOpen}
            aria-controls="theme-menu"
            aria-haspopup="true"
          >
            Theme
          </Button>
          <Menu
            id="theme-menu"
            anchorEl={anchorEl}
            keepMounted
            open={Boolean(anchorEl)}
            onClose={handleThemeMenuClose}
          >
            {Object.keys(themes).map((theme, index) => (
              <MenuItem key={index} onClick={() => handleThemeChange(theme)}>
                {theme}
              </MenuItem>
            ))}
          </Menu>
          <StatusBall
            status={serverRunning ? "good" : "bad"}
            additionalInfo={
              "Video creation server down since: " + Date(lastServerRun)
            }
          />
        </Toolbar>
      </AppBar>
    </div>
  );
}
