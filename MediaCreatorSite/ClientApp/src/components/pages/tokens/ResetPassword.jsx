import React, { useState } from 'react';
import { makeStyles } from '@mui/styles';
import { Button, CircularProgress, Container, TextField } from '@mui/material';
import { _database } from '../../../services/databaseService';
import { _notify } from '../../../services/notifyService';
import { useSearchParams } from 'react-router-dom';

const useStyles = makeStyles((theme) => ({
  root: {
    display: 'flex !important',
    width: '100%',
    flexDirection: 'column',
    alignItems: 'center',
    [theme.breakpoints.down('md')]: {
      margin: '20px auto',
    },
    [theme.breakpoints.up('lg')]: {
      margin: '100px auto',
    },
    height: '100vh',
  },
  contentContainer:{
    display: 'flex !important',
    width: '100%',
    flexDirection: 'column',
    alignItems: 'center',
  },
  button: {
    margin: '20px auto !important'
  },
  textField: {
    margin: '10px auto !important',
  },
}));

const ResetPassword = () => {
    const classes = useStyles();
    const [searchParams] = useSearchParams();
    const email = searchParams.get("email");

    const [submit, setSubmit] = useState(false);
    const [password, setPassword] = useState('');
    const [confirmedPassword, setConfirmedPassword] = useState('');

    const handleReset = () =>{
      //Ensure username and password are of proper standered and the
      if(!submit){
        setSubmit(true);
        if(password !== confirmedPassword){
          _notify.error("Passwords do not match");
          return;
        }
        let token = searchParams.get("token").replaceAll(' ', '+');
        _database.api("POST", "/auth/ResetPassword", {email, password, token}).then(result => {
            if(result){
              if(result.status === 1){
                _notify.success("Reset Password!", 2000, () =>{window.location = "/"});
              }else{
                _notify.error(result.errorResult);
              }
            }
            else{
              _notify.error("Could not get session. A unexpected error occurred, check your network connection.");
            }
            setSubmit(false);
          });
      }
    }

    return (
        <Container className={classes.root} maxWidth="md">
      {submit ? (
        <CircularProgress />
      ) : (
        <>
          <Container className={classes.contentContainer}>
          <TextField
              className={classes.textField}
              type="password"
              label="Password"
              value={password}
              onChange={e => setPassword(e.target.value)}
          />
          <TextField
              className={classes.textField}
              type="password"
              label="Confirmed Password"
              value={confirmedPassword}
              onChange={e => setConfirmedPassword(e.target.value)}
          />
          <Button className={classes.button} variant="contained" color="primary" onClick={handleReset}>
              Reset
          </Button>
          </Container>
        </>
      )}
    </Container>
    );
};

export default ResetPassword;
