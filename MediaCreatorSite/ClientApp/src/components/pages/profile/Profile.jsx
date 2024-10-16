import React, { useState, useContext, useEffect } from "react";
import { makeStyles } from "@mui/styles";
import { Button, CircularProgress, Container, Typography } from "@mui/material";
import { SessionContext } from "../../providers/SessionProvider";
import { _database } from "../../../services/databaseService";
import Loader from "../../loaders/Loader";
import { _notify } from "../../../services/notifyService";
import { BaseModalContext } from "../../providers/BaseModalProvider";
import { useSearchParams } from "react-router-dom";

const useStyles = makeStyles((theme) => ({
  root: {
    display: "flex !important",
    width: "100%",
    flexDirection: "column",
    alignItems: "center",
    [theme.breakpoints.down("md")]: {
      margin: "20px auto"
    },
    [theme.breakpoints.up("lg")]: {
      margin: "100px auto"
    },
    height: "100vh"
  },
  button: {
    width: "fit-content",
    height: "fit-content",
    fontSize: "1.5rem",
    textAlign: "center",
    margin: "40px auto auto auto !important"
  },
  contentContainer: {
    display: "flex !important",
    flexDirection: "column",
    alignItems: "center",
    width: "100%",
    margin: "0px auto !important",
    justifyContent: "center"
  },
  infoContainer: {
    display: "flex",
    flexDirection: "column",
    alignItems: "left",
    width: "100%",
    margin: "10px auto !important"
  },
  typography: {
    [theme.breakpoints.up("lg")]: {
      fontSize: "30px !important" // Font size for small screens and below
    }
  }
}));

const Profile = () => {
  const classes = useStyles();
  const [credits, setCredits] = useState(0);
  const [initialLoad, setInitialLoad] = useState(false);
  const [searchParams] = useSearchParams();
  const [profileInfoLoading, setProfileInfoLoading] = useState(false);
  //   const [apiKey, setApiKey] = useState("");

  const { sessionData } = useContext(SessionContext);
  const { setContent, show, setSize } = useContext(BaseModalContext);

  const handleBuyMoreCredits = () => {
    setSize("lg");
    setContent("CreditPayment");
    show();
  };

  const handleResetingPassword = () => {
    setProfileInfoLoading(true);
    _database
      .api("POST", "/auth/SendResetPasswordEmail", {
        email: sessionData.user.email
      })
      .then((result) => {
        if (result) {
          if (result.status === 1) {
            _notify.success("Email Sent!");
          } else {
            _notify.error(result.errorResult);
          }
        } else {
          _notify.error(
            "Could not get session. A unexpected error occurred, check your network connection."
          );
        }
        setProfileInfoLoading(false);
      });
  };

  const handleEmailConfirmation = () => {
    _database
      .api("POST", "/auth/SendConfirmationEmail", {
        userId: sessionData.user.id
      })
      .then((result) => {
        if (result) {
          if (result.status === 1) {
            _notify.success("Email Sent!");
          } else {
            _notify.error(result.errorResult);
          }
        } else {
          _notify.error(
            "Could not get session. A unexpected error occurred, check your network connection."
          );
        }
      });
  };

  const updateInfo = (cb) => {
    _database.api("Get", "/profile").then((result) => {
      if (result) {
        if (result.status === 1) {
          setCredits(result.data.credits);
        } else {
          _notify.error(result.errorResult);
        }
      } else {
        _notify.error(
          "Could not get session. A unexpected error occurred, check your network connection."
        );
      }
      cb();
    });
  };

  useEffect(() => {
    updateInfo(() => {
      let showCredit = searchParams.get("purchase");
      if (showCredit === "1") {
        handleBuyMoreCredits();
        // Remove "purchase" from the search parameters
        searchParams.delete("purchase");
        // Replace the current URL with the modified search parameters
        window.history.replaceState(
          {},
          "",
          `${window.location.pathname}?${searchParams.toString()}`
        );
      }
      setInitialLoad(true);
    });
  }, []);

  return (
    <Container className={classes.root} maxWidth="md">
      {!sessionData || !initialLoad ? (
        <CircularProgress />
      ) : (
        <>
          <Container className={classes.contentContainer}>
            <Typography className={classes.typography}>
              {sessionData?.user?.email}
            </Typography>
            <Typography className={classes.typography}>
              Credits: {credits}
            </Typography>
            <Button
              className={classes.button}
              variant="standered"
              color="primary"
              size="large"
              onClick={handleBuyMoreCredits}
            >
              Buy More Credits
            </Button>

            <Container
              className={classes.contentContainer}
              style={{ display: "flex" }}
            >
              <Button
                className={classes.button}
                variant="standered"
                color="primary"
                size="large"
                onClick={handleResetingPassword}
                disabled={profileInfoLoading} // disable the button when generating
              >
                {profileInfoLoading ? (
                  <Loader message="Processing..." />
                ) : (
                  "Reset Password"
                )}
              </Button>
              {!sessionData?.user?.email_confirmed ? (
                <Button
                  className={classes.button}
                  variant="standered"
                  color="primary"
                  size="large"
                  onClick={handleEmailConfirmation}
                  disabled={profileInfoLoading} // disable the button when generating
                >
                  {profileInfoLoading ? (
                    <Loader message="Processing..." />
                  ) : (
                    "Confirm Email"
                  )}
                </Button>
              ) : (
                <></>
              )}

              {/* <Container className={classes.infoContainer}>
                    <Typography>API KEY:</Typography><Typography>{apiKey}</Typography>
                </Container> */}
            </Container>

            <Button
              className={classes.button}
              variant="standered"
              color="primary"
              size="large"
              href="/legal/terms-of-service"
            >
              Terms Of Service & Privary Policy
            </Button>
          </Container>
        </>
      )}
    </Container>
  );
};

export default Profile;
