import React, { useState, useEffect } from "react";
import { makeStyles } from "@mui/styles";
import {
  TextField,
  FormControl,
  InputLabel,
  Select,
  MenuItem
} from "@mui/material";
import localStorageService from "../../../../services/localStorageService";

const useStyles = makeStyles((theme) => ({
  optionsContainer: {
    display: "flex",
    justifyContent: "center",
    alignItems: "center",
    flexDirection: "column",
    margin: "20px auto !important"
  },
  input: {
    margin: "10px auto !important",
    width: "200px"
  }
}));

const Options = ({ handleOptionsChange }) => {
  const classes = useStyles();
  const [width, setWidth] = useState(
    localStorageService.getItem("width") ?? 500
  );
  const [height, setHeight] = useState(
    localStorageService.getItem("height") ?? 500
  );
  const [voice, setVoice] = useState(
    localStorageService.getItem("voice") ?? "Male"
  );
  const [pictureAi, setPictureAi] = useState(
    localStorageService.getItem("pictureAi") ?? "DeepAI"
  );
  const [resolution, setResolution] = useState(
    localStorageService.getItem("resolution") ?? "256x256"
  );

  const handleWidthChange = (event) => {
    setWidth(event.target.value);
    let value = parseInt(event.target.value);
    if (value > 128 && value < 1536)
      localStorageService.setItem("width", value);
    handleOptionsChange({ width: value, height, voice, pictureAi, resolution });
  };

  const handleHeightChange = (event) => {
    setHeight(event.target.value);
    let value = parseInt(event.target.value);
    if (value > 128 && value < 1536)
      localStorageService.setItem("height", value);
    handleOptionsChange({ width, height: value, voice, pictureAi, resolution });
  };

  const handleVoiceChange = (event) => {
    setVoice(event.target.value);
    localStorageService.setItem("voice", event.target.value);
    handleOptionsChange({
      width,
      height,
      voice: event.target.value,
      pictureAi,
      resolution
    });
  };

  const handlePictureAiChange = (event) => {
    setPictureAi(event.target.value);
    localStorageService.setItem("pictureAi", event.target.value);
    handleOptionsChange({
      width,
      height,
      voice,
      pictureAi: event.target.value,
      resolution
    });
  };

  const handleResolutionChange = (event) => {
    setResolution(event.target.value);
    localStorageService.setItem("resolution", event.target.value);
    handleOptionsChange({
      width,
      height,
      voice,
      pictureAi,
      resolution: event.target.value
    });
  };

  useEffect(() => {
    handleOptionsChange({ width, height, voice, pictureAi, resolution });
  }, []);

  return (
    <div className={classes.optionsContainer}>
      <FormControl className={classes.input}>
        <InputLabel>Picture Ai</InputLabel>
        <Select value={pictureAi} onChange={handlePictureAiChange}>
          <MenuItem value="DeepAI">DeepAI</MenuItem>
          <MenuItem value="ChatGPT">ChatGPT</MenuItem>
        </Select>
      </FormControl>
      {pictureAi === "DeepAI" ? (
        <>
          <TextField
            className={classes.input}
            label="Width"
            type="number"
            InputProps={{ inputProps: { min: 128, max: 1536 } }}
            value={width}
            onChange={handleWidthChange}
          />
          <TextField
            className={classes.input}
            label="Height"
            type="number"
            InputProps={{ inputProps: { min: 128, max: 1536 } }}
            value={height}
            onChange={handleHeightChange}
          />
        </>
      ) : (
        <>
          <FormControl className={classes.input}>
            <InputLabel>Resolution</InputLabel>
            <Select value={resolution} onChange={handleResolutionChange}>
              <MenuItem value="256x256">256x256</MenuItem>
              <MenuItem value="512x512">512x512</MenuItem>
              <MenuItem value="1024x1024">1024x1024</MenuItem>
            </Select>
          </FormControl>
        </>
      )}

      <FormControl className={classes.input}>
        <InputLabel>Voice</InputLabel>
        <Select value={voice} onChange={handleVoiceChange}>
          <MenuItem value="Male">Male</MenuItem>
          <MenuItem value="Female">Female</MenuItem>
        </Select>
      </FormControl>
    </div>
  );
};

export default Options;
