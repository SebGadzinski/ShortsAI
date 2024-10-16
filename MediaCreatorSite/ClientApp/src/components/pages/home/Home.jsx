import React, { useState, useContext } from "react";
import { makeStyles } from "@mui/styles";
import { Button, CircularProgress, Container, TextField } from "@mui/material";
import { SessionContext } from "../../providers/SessionProvider";
import TitleText from "../../shared/TitleText";
import { _database } from "../../../services/databaseService";
import Options from "./components/Options";
import Loader from "../../loaders/Loader";
import { _notify } from "../../../services/notifyService";
import { ServerStatusContext } from "../../providers/ServerStatusProvider";

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
  input: {
    padding: "10px",
    width: "100%",
    [theme.breakpoints.down("lg")]: {
      width: "80%"
    },
    textAlign: "center",
    marginBottom: theme.spacing(4)
  },
  button: {
    width: "300px",
    [theme.breakpoints.down("lg")]: {
      width: "200px" // Font size for small screens and below
    },
    height: "fit-content",
    fontSize: "1.5rem",
    margin: "40px auto auto auto !important"
  },
  titleContainer: {
    display: "flex !important",
    width: "100%",
    margin: "0px auto !important",
    height: "200px !important",
    justifyContent: "center"
  },
  contentContainer: {
    display: "flex !important",
    flexDirection: "column",
    alignItems: "center",
    width: "100%",
    margin: "0px auto !important",
    justifyContent: "center"
  },
  optionsContainer: {
    display: "none !important"
  },
  showOptionsContainer: {
    display: "flex !important",
    flexDirection: "column",
    alignItems: "center",
    width: "100%",
    margin: "0px auto !important",
    justifyContent: "center"
  },
  warpMessage: {
    textAlign: "center",
    marginTop: "5px !important"
  }
}));

const Home = ({ handleAuthModalOpen, handleAuthModalClose }) => {
  const classes = useStyles();
  const messages = [
    "Welcome to Shorts AI",
    "Please Enter A Title For Your Video",
    "And Wait For The Magic..."
  ];
  const [inputValue, setInputValue] = useState("");
  const [generating, setGenerating] = useState(false);
  const [options, setOptions] = useState({}); // Add this line
  const [showOptions, setShowOptions] = useState(false); // Track the visibility of the Options component

  const { sessionData, isLoggedIn } = useContext(SessionContext);
  const { serverRunning, lastServerRun } = useContext(ServerStatusContext);

  const handleInputChange = (e) => {
    setInputValue(e.target.value);
  };

  const handleOptionsChange = (newOptions) => {
    // Add this function
    setOptions(newOptions);
  };

  const handleGenerateClick = async () => {
    if (isLoggedIn()) {
      if (inputValue && inputValue.length > 5) {
        setGenerating(true);

        //Filter the options
        let width = options.width;
        let height = options.height;

        if (options.pictureAi === "ChatGPT") {
          let resolutionSplit = options.resolution.split("x");
          width = parseInt(resolutionSplit[0]);
          height = parseInt(resolutionSplit[1]);
        }

        let reqOptions = {
          title: inputValue,
          width: width,
          height: height,
          picture_store: options.pictureAi,
          voice: options.voice
        };

        _database
          .api("POST", "/home/CreateVideo", reqOptions)
          .then((result) => {
            if (result) {
              if (result.status === 1) {
                _notify.success(
                  "Credit Will Be Charged On Completion!",
                  500,
                  () => {
                    window.location = "/all-videos";
                  }
                );
              } else if (result.errorCode === 116) {
                window.location = "/my-profile?purchase=1";
              } else {
                _notify.error(result.errorResult);
              }
            } else {
              _notify.error(
                "Could not get session. A unexpected error occurred, check your network connection."
              );
            }
          });
        setGenerating(false);
      } else {
        _notify.error("Title must be at least 5 characters");
      }
    } else {
      handleAuthModalOpen();
    }
  };

  const handleOptionsButtonClick = () => {
    setShowOptions(!showOptions); // Toggle the visibility of the Options component
  };

  return (
    <Container className={classes.root} maxWidth="md">
      {!sessionData ? (
        <CircularProgress />
      ) : (
        <>
          <Container className={classes.titleContainer}>
            <TitleText messages={messages} />
          </Container>
          <Container className={classes.contentContainer}>
            <TextField
              className={classes.input}
              variant="standard"
              placeholder="Enter a title for a short..."
              value={inputValue}
              inputProps={{ maxLength: 50 }}
              onChange={handleInputChange}
              multiline
              rows={inputValue.split("\n").length || 1}
              rowsMax={4}
            />
            <Button
              className={classes.button}
              variant="standered"
              color="primary"
              size="large"
              onClick={
                serverRunning
                  ? handleGenerateClick
                  : () =>
                      _notify.error(
                        "Video creation server down since: " + lastServerRun
                      )
              }
              disabled={generating} // disable the button when generating
            >
              {generating ? (
                <Loader message="Can take up to 5 minutes..." />
              ) : (
                "Generate"
              )}
            </Button>
            <Button
              className={classes.button} // Use the same style as the generate button
              variant="standered"
              color="primary"
              size="large"
              onClick={handleOptionsButtonClick} // Call the new button click handler
            >
              {showOptions ? "Hide Options" : "Show Options"}
            </Button>
            <Container
              className={
                showOptions
                  ? classes.showOptionsContainer
                  : classes.optionsContainer
              }
            >
              <Options handleOptionsChange={handleOptionsChange} />
            </Container>
          </Container>
        </>
      )}
    </Container>
  );
};

export default Home;
