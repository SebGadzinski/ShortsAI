import React, { useEffect, useState } from 'react';
import { Typography } from '@mui/material';
import { makeStyles } from '@mui/styles';

const useStyles = makeStyles((theme) => ({
  typography: {
    fontSize: '4rem !important', // Default font size
    [theme.breakpoints.down('lg')]: {
      fontSize: '2rem !important', // Font size for small screens and below
    },
    textAlign: 'center'
  },
}));

const TitleText = ({ messages }) => {
  const [typedMessage, setTypedMessage] = useState('');
  const [currentMessageIndex, setCurrentMessageIndex] = useState(0);
  const [currentCharIndex, setCurrentCharIndex] = useState(0);
  const [isPaused, setIsPaused] = useState(false);
  const classes = useStyles();

  useEffect(() => {
    const currentMessage = messages[currentMessageIndex];
    const totalChars = currentMessage.length;

    const timer = setInterval(() => {
      if (currentCharIndex < totalChars) {
        setTypedMessage((prevMessage) => prevMessage + currentMessage[currentCharIndex]);
        setCurrentCharIndex((prevIndex) => prevIndex + 1);
      } else if (!isPaused) {
        setIsPaused(true);
        setTimeout(() => {
          setIsPaused(false);
          setCurrentCharIndex(0);
          setTypedMessage('');
          setCurrentMessageIndex((prevIndex) => (prevIndex + 1) % messages.length);
        }, 1000); // Pause for 1 second (adjust the duration as needed)
      }
    }, getRandomTypingDelay());

    return () => {
      clearInterval(timer);
    };
  }, [currentCharIndex, currentMessageIndex, isPaused, messages]);

  const getRandomTypingDelay = () => {
    // Generate a random delay between 150ms and 350ms
    return Math.floor(Math.random() * 100) + 50;
  };

  return (
    <Typography className={classes.typography}>
      {typedMessage}
    </Typography>
  );
};

export default TitleText;
