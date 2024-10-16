import React, { useState, useEffect, useRef, useContext } from "react";
import { makeStyles } from "@mui/styles";
import { Button, Card, Typography, Container } from "@mui/material";
import Loader from "../../../loaders/Loader";
import { _database } from "../../../../services/databaseService";
import { _notify } from "../../../../services/notifyService";
import { SessionContext } from "../../../providers/SessionProvider";
import Swal from "sweetalert2";

const useStyles = makeStyles((theme) => ({
  contentContainer: {
    display: "flex !important",
    flexDirection: "column",
    width: "100%",
    margin: "5px auto !important",
    textAlign: "center",
    alignItems: "center",
    justifyContent: "space-between !important",
    padding: "5px",
    height: "100%"
  },
  card: {
    display: "flex !important",
    flexDirection: "column",
    width: "300px",
    [theme.breakpoints.down("lg")]: {
      margin: "10px 0px !important"
    },
    [theme.breakpoints.up("lg")]: {
      height: "200px"
    },
    justifyContent: "center",
    backgroundColor: theme.palette.card.primary,
    padding: "10px",
    margin: "10px 5px !important"
  },
  title: {
    [theme.breakpoints.up("lg")]: {
      fontSize: "20px !important" // Font size for small screens and below
    },
    textAlign: "center",
    borderRadius: "10px",
    padding: "5px",
    backgroundColor: theme.palette.mode === "light" ? "white" : "black"
  },
  button: {
    height: "fit-content",
    fontSize: "1.5rem",
    color: "white !important",
    backgroundColor: `${theme.palette.secondary.main} !important`
  }
}));

const CollectableVideo = ({ id, initialStatus, title }) => {
  const classes = useStyles();
  const [loadingMessage, setLoadingMessage] = useState("Getting Info...");
  const [status, setStatus] = useState(initialStatus.toLowerCase());
  const [downloading, setDownloading] = useState(false);
  const [uploadingToYoutube, setUploadingToYoutube] = useState(false);
  const intervalIdRef = useRef(null); // Use a ref to store the interval reference
  const { sessionData } = useContext(SessionContext);

  const updateCardInfo = () => {
    _database.api("Post", "/video/GetVideoInfo", { id }).then((result) => {
      if (result) {
        if (result.status === 1) {
          //Update info
          if (result.data.status === "processing")
            setLoadingMessage("Processing Video...");
          else if (result.data.status === "waiting")
            setLoadingMessage("In Queue To Process...");

          setStatus(result.data.status);
          if (result.data.status === "complete") {
            clearInterval(intervalIdRef.current);
          }
        } else {
          _notify.error("Error Updating Card: " + result.errorResult);
        }
      } else {
        _notify.error(
          "Error Updating Card: Could not get session. A unexpected error occurred, check your network connection."
        );
      }
    });
  };

  const downloadVideo = async () => {
    setDownloading(true);
    await _database.downloadVideo({ id, title });
    setDownloading(false);
  };

  const uploadToYoutube = async () => {
    Swal.fire({
      title: "Enter the playlist it would be in",
      input: "text",
      inputPlaceholder: "Random",
      showCancelButton: true,
      confirmButtonText: "Submit",
      showLoaderOnConfirm: true,
      preConfirm: (category) => {
        setUploadingToYoutube(true);
        _database
          .api("POST", "/video/UploadToYoutube", { id, category })
          .then((result) => {
            if (result) {
              if (result.status === 1) {
                _notify.success("Youtube uploading video as we speak!");
              } else {
                _notify.error(result.errorResult);
              }
            } else {
              _notify.error(
                "Could not get session. A unexpected error occurred, check your network connection."
              );
            }
            setUploadingToYoutube(false);
          });
      },
      allowOutsideClick: () => !Swal.isLoading()
    });
  };

  useEffect(() => {
    //If the status of this video ard is set to "waiting or processing", set a timer interval to update it until it is "collectable"
    if (status !== "complete") {
      const id = setInterval(updateCardInfo, 10000); // Store the interval reference
      intervalIdRef.current = id; // Update the ref with the interval reference
    }

    // Clear the interval when the component is unmounted
    return () => {
      clearInterval(intervalIdRef.current);
    };
  }, []);

  return (
    <Card className={classes.card}>
      <Container className={classes.contentContainer}>
        {status === "processing" || status === "waiting" ? (
          <Loader message={loadingMessage} />
        ) : (
          <></>
        )}
        {status === "complete" && sessionData?.user?.email === "Youtube" ? (
          !uploadingToYoutube ? (
            <Button className={classes.button} onClick={uploadToYoutube}>
              Upload To Youtube
            </Button>
          ) : (
            <Loader message={"Sending Request..."} />
          )
        ) : (
          <></>
        )}
        {status === "complete" ? (
          !downloading ? (
            <Button className={classes.button} onClick={downloadVideo}>
              Download
            </Button>
          ) : (
            <Loader message={"Downloading..."} />
          )
        ) : (
          <></>
        )}
        <br></br>
        <Typography className={classes.title}>{title}</Typography>
      </Container>
    </Card>
  );
};

export default CollectableVideo;
