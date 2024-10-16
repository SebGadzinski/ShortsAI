import React, { useState } from 'react';
import { Modal, Button, Box, CircularProgress, Container } from '@mui/material';
import SignUp from '../shared/SignUp';
import Login from '../shared/Login';
import { makeStyles } from '@mui/styles';

const useStyles = makeStyles((theme) => ({
  container: {
    display: 'flex !important',
    flexDirection: 'column',
    width: '100%',
    margin: '0px auto !important',
    justifyContent: 'center',
  },
  loader: {
    margin: 'auto !important'
  }
}));

const AuthModal = ({ open, onClose }) => {
  const classes = useStyles(useStyles);
  const [showLogin, setShowLogin] = useState(true);
  const [submit, submitSet] = useState(false);

  const handleToggleLoginSignUp = () => {
    setShowLogin(!showLogin);
  };

  const handleClose = () => {
    setShowLogin(true); // Reset to default state when closing the modal
    onClose();
  };

  return (
    <Modal open={open} onClose={handleClose}>
      <Box
        sx={{
          position: 'absolute',
          top: '50%',
          left: '50%',
          transform: 'translate(-50%, -50%)',
          width: 400,
          bgcolor: 'background.paper',
          border: '2px solid #000',
          boxShadow: 24,
          p: 4,
        }}
      >
        <Container className={classes.container} >
          { submit ? (
            <CircularProgress className={classes.loader} />
          ) : (
            <>
            { showLogin
              ? <Login handleClose={handleClose} submit={submit} submitSet={submitSet}  />
              : <SignUp handleClose={handleClose} submit={submit} submitSet={submitSet} />
            }
            <Button variant="contained" color="primary" onClick={handleToggleLoginSignUp}>
              {showLogin ? 'Switch to Sign Up' : 'Switch to Login'}
            </Button>
            </>
            )}
        </Container>
      </Box>
    </Modal>
  );
};

export default AuthModal;
