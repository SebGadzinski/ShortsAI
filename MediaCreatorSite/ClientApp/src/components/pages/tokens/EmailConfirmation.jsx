import React, { useContext, useState } from 'react';
import { makeStyles } from '@mui/styles';
import { Button, CircularProgress, Container, TextField } from '@mui/material';
import localStorageService from '../../../services/localStorageService';
import { _database } from '../../../services/databaseService';
import { _notify } from '../../../services/notifyService';
import { SessionContext } from '../../providers/SessionProvider';
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

const EmailConfirmation = () => {
    const classes = useStyles();
    const [searchParams] = useSearchParams();
    const [submit, setSubmit] = useState(false);
    const [email, setEmail] = useState(searchParams.get("email") ?? localStorageService.getItem('email') ?? "");
    const [password, setPassword] = useState('');
    const { updateSessionData } = useContext(SessionContext);

    const handleConfirm = () =>{
      //Ensure username and password are of proper standered and the
      if(!submit){
        setSubmit(true);
        _database.api("POST", "/auth/Login", {email: email, password, deviceName: navigator.userAgent}).then(result => {
            if(result){
              if(result.status === 1){
                let token = searchParams.get("token").replaceAll(' ', '+');
                updateSessionData(result.data);
                localStorageService.setItem('email', email);
                _database.api("POST", "/auth/ConfirmEmail", {token}).then(secondResult => {
                  if(secondResult){
                    if(secondResult.status === 1){
                      updateSessionData(result.data);
                      localStorageService.setItem('email', email);
                      window.location = "/";
                    }else{
                      _notify.error(secondResult.errorResult);
                    }
                  }
                  else{
                    _notify.error("Could not get session. A unexpected error occurred, check your network connection.");
                  }
                });
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
              variant="outlined"
              label="Email"
              value={email}
              onChange={e => setEmail(e.target.value)}
          />
          <TextField
              className={classes.textField}
              type="password"
              label="Password"
              value={password}
              onChange={e => setPassword(e.target.value)}
          />
          <Button className={classes.button} variant="contained" color="primary" onClick={handleConfirm}>
              Confirm
          </Button>
          </Container>
        </>
      )}
    </Container>
    );
};

export default EmailConfirmation;
