import React, { useContext } from "react";
import { Modal, Box, CircularProgress, Container } from "@mui/material";
import { makeStyles } from "@mui/styles";
import { BaseModalContext } from "../providers/BaseModalProvider";

const useStyles = makeStyles((theme) => ({
  container: {
    display: "flex !important",
    flexDirection: "column",
    width: "100%",
    margin: "0px auto !important",
    justifyContent: "center"
  },
  loader: {
    margin: "auto !important"
  },
  xs: {
    width: "400px !important"
  },
  fit: {
    width: "fit-content !important"
  },
  lg: {
    width: "80% !important"
  }
}));

const BaseModal = ({ children }) => {
  const classes = useStyles(useStyles);
  const { open, hide, loading, size } = useContext(BaseModalContext);

  return (
    <Modal open={open} onClose={hide}>
      <Box
        className={classes[size]}
        sx={{
          position: "absolute",
          top: "50%",
          left: "50%",
          transform: "translate(-50%, -50%)",
          bgcolor: "background.paper",
          border: "2px solid #000",
          boxShadow: 24,
          p: 4
        }}
      >
        <Container className={classes.container}>
          {loading ? <CircularProgress className={classes.loader} /> : children}
        </Container>
      </Box>
    </Modal>
  );
};

export default BaseModal;
