import React, { useState, useContext, useEffect } from "react";
import { makeStyles } from "@mui/styles";
import { CircularProgress, Container, Typography } from "@mui/material";
import { DataGrid } from "@mui/x-data-grid";
import { SessionContext } from "../../providers/SessionProvider";
import { _database } from "../../../services/databaseService";
import { _notify } from "../../../services/notifyService";
import CollectableVideo from "./components/CollectableVideo";

const useStyles = makeStyles((theme) => ({
  root: {
    display: "flex !important",
    width: "100%",
    flexDirection: "column",
    alignItems: "center",
    [theme.breakpoints.down("lg")]: {
      margin: "20px auto"
    },
    [theme.breakpoints.up("lg")]: {
      margin: "100px auto"
    },
    height: "fit-content"
  },
  contentContainer: {
    display: "flex !important",
    flexDirection: "column",
    alignItems: "center",
    width: "100%",
    margin: "0px auto !important",
    justifyContent: "center"
  },
  collectablesContainer: {
    display: "flex !important",
    flexDirection: "row",
    flexWrap: "wrap",
    alignItems: "center",
    width: "100%",
    margin: "0px auto !important",
    justifyContent: "center"
  },
  dataGrid: {
    width: "100%"
  },
  message: {
    borderRadius: "10px",
    backgroundColor: theme.palette.secondary.main,
    padding: "10px",
    color: "white"
  }
}));

const ShowMobileCols = () => {
  const checkMobile = window.matchMedia("(max-width: 900px)");
  return !checkMobile.matches;
};

const columns = [
  { field: "status", headerName: "Status", flex: 1 },
  { field: "title", headerName: "Title", flex: 2 },
  {
    field: "pictureStore",
    headerName: "Picture Store",
    flex: 1
  },
  { field: "voice", headerName: "Voice", flex: 1 },
  { field: "width", headerName: "Width", flex: 1 },
  { field: "height", headerName: "Height", flex: 1 },
  {
    field: "createdOn",
    headerName: "Created On",
    flex: 2
  }
];

const columnVisibility = {
  status: true,
  title: true,
  pictureStore: ShowMobileCols(),
  voice: ShowMobileCols(),
  width: ShowMobileCols(),
  height: ShowMobileCols(),
  createdOn: ShowMobileCols()
};

const Videos = () => {
  const classes = useStyles();
  const [initialLoad, setInitialLoad] = useState(false);
  const [collectableVideos, setCollectableVideos] = useState([]);
  const [rows, setRows] = useState([]);

  const { sessionData } = useContext(SessionContext);

  const populateTable = async () => {
    let stateChanges = { videoRows: [], collectableVideos: [] };
    let result = await _database.api("Get", "/video");
    if (result) {
      if (result.status === 1) {
        stateChanges.videoRows = result.data.historyVideos.map((x) => {
          let date = new Date(x.createdOn);
          x.createdOn = `${date.toLocaleDateString()} ${date.toLocaleTimeString()}`;
          return x;
        });
        stateChanges.collectableVideos = result.data.collectableVideos;
      } else {
        _notify.error(result.errorResult);
      }
    } else {
      _notify.error(
        "Could not get session. A unexpected error occurred, check your network connection."
      );
    }

    setRows(stateChanges.videoRows);
    setCollectableVideos(stateChanges.collectableVideos);
    setInitialLoad(true);
  };

  useEffect(() => {
    populateTable();
  }, [sessionData]);

  return (
    <Container className={classes.root}>
      {!initialLoad ? (
        <CircularProgress />
      ) : (
        <>
          {/* //TODO  Random video to watch */}
          <Typography className={classes.message}>
            Enable Pop Ups To Download!!
          </Typography>
          <br></br>
          <Container className={classes.contentContainer}>
            {collectableVideos.length > 0 ? (
              <Container className={classes.collectablesContainer}>
                {collectableVideos.map((video, index) => (
                  <CollectableVideo
                    key={index}
                    id={video.id}
                    initialStatus={video.status}
                    title={video.title}
                  />
                ))}
              </Container>
            ) : (
              <></>
            )}

            <br></br>
            <Typography className={classes.message}>
              Videos Delete After A Hour!
            </Typography>
            <br></br>
          </Container>
        </>
      )}
      <br></br>
      <DataGrid
        className={classes.dataGrid}
        rows={rows}
        columns={columns}
        columnVisibilityModel={columnVisibility}
        pageSize={5}
        rowsPerPageOptions={[5, 10, 25, 50]}
        virtualization={true}
        sortModel={[
          {
            field: "createdOn",
            sort: "desc"
          }
        ]}
        autoHeight={true}
      />
    </Container>
  );
};

export default Videos;
