import React, { useState, useContext } from "react";
import {
  Button,
  TextField,
  Container,
  FormControlLabel,
  Checkbox
} from "@mui/material";
import { _database } from "../../services/databaseService";
import { _notify } from "../../services/notifyService";
import { SessionContext } from "../providers/SessionProvider";
import { makeStyles } from "@mui/styles";
import localStorageService from "../../services/localStorageService";
import { Link } from "react-router-dom";

const useStyles = makeStyles((theme) => ({
  container: {
    display: "flex !important",
    flexDirection: "column",
    width: "100%",
    margin: "0px auto !important",
    justifyContent: "center"
  },
  button: {
    margin: "20px auto !important"
  },
  textField: {
    margin: "10px auto !important"
  }
}));

const Login = ({ handleClose, submit, submitSet }) => {
  const classes = useStyles(useStyles);
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [confirmedPassword, setConfirmedPassword] = useState("");
  const [termsAccepted, setTermsAccepted] = useState(false); // state to handle terms acceptance
  const { updateSessionData } = useContext(SessionContext);

  const handleSignUp = () => {
    //Ensure they accepted policies
    if (!termsAccepted) {
      _notify.error("You must accept the terms of service.");
      return;
    }
    //Ensure username and password are of proper standered and the
    if (!submit) {
      submitSet(true);
      _database
        .api("POST", "/auth/SignUp", {
          email,
          password,
          confirmedPassword,
          phoneNumber: "",
          deviceName: navigator.userAgent,
          claims: []
        })
        .then((result) => {
          if (result) {
            if (result.status === 1) {
              updateSessionData(result.data);
              localStorageService.setItem("email", email);
              _notify.success(
                "Check Your Email Inbox And Verify Email Address",
                500,
                () => {
                  window.location = "/";
                }
              );
            } else {
              submitSet(false);
              _notify.error(result.errorResult);
            }
          } else {
            submitSet(false);
            _notify.error(
              "Could not get session. A unexpected error occurred, check your network connection."
            );
          }
        });
    }
  };

  return (
    <Container className={classes.container} maxWidth="xs">
      <h2>Sign Up</h2>
      <TextField
        className={classes.textField}
        variant="outlined"
        label="Email"
        value={email}
        onChange={(e) => setEmail(e.target.value)}
      />
      <TextField
        className={classes.textField}
        type="password"
        label="Password"
        value={password}
        onChange={(e) => setPassword(e.target.value)}
      />
      <TextField
        className={classes.textField}
        type="password"
        label="Confirmed Password"
        value={confirmedPassword}
        onChange={(e) => setConfirmedPassword(e.target.value)}
      />
      <FormControlLabel
        control={
          <Checkbox
            checked={termsAccepted}
            onChange={(e) => setTermsAccepted(e.target.checked)}
            name="termsAccepted"
            color="secondary"
          />
        }
        label={
          <>
            <span style={{ "font-size": "14px" }}>I accept the </span>
            <Link
              style={{ "font-size": "14px" }}
              to="/legal/terms-of-service"
              target="_blank"
            >
              Terms of Service
            </Link>
          </>
        }
      />
      <Button
        className={classes.button}
        variant="contained"
        color="primary"
        onClick={handleSignUp}
      >
        Sign Up
      </Button>
    </Container>
  );
};

export default Login;
