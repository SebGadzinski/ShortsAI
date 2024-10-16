import React from "react";
import { makeStyles } from "@mui/styles";
import { _notify } from "../../services/notifyService";

const useStyles = makeStyles({
  bubble: {
    width: "30px",
    height: "30px",
    borderRadius: "50%",
    animation: "$pulse 1.5s infinite",
    cursor: "pointer"
  },
  "@keyframes pulse": {
    "0%": {
      transform: "scale(0.95)",
      opacity: 0.7
    },
    "70%": {
      transform: "scale(1)",
      opacity: 0.85
    },
    "100%": {
      transform: "scale(0.95)",
      opacity: 0.7
    }
  },
  good: {
    backgroundColor: "green"
  },
  bad: {
    backgroundColor: "red"
  }
});

const StatusBall = ({ status, additionalInfo }) => {
  const classes = useStyles();

  return (
    <div
      className={`${status === "bad" ? classes.bubble : ""} ${
        status === "good" ? classes.good : classes.bad
      }`}
      title={additionalInfo}
      onClick={
        status === "bad" ? () => _notify.error(additionalInfo) : () => {}
      }
    ></div>
  );
};

export default StatusBall;
