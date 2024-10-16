import React, { useState, useContext, useEffect} from 'react';
import { Button, TextField, Container } from '@mui/material';
import { _database } from '../../services/databaseService';
import { _notify } from '../../services/notifyService';
import { SessionContext } from '../providers/SessionProvider';
import { makeStyles } from '@mui/styles';
import localStorageService from '../../services/localStorageService';

const useStyles = makeStyles((theme) => ({
  container: {
    display: 'flex !important',
    flexDirection: 'column',
    width: '100%',
    margin: '0px auto !important',
    justifyContent: 'center',
  },
  button: {
    margin: '20px auto !important'
  },
  textField: {
    margin: '10px auto !important',
  },
}));

const Login = ({handleClose, submit, submitSet}) => {
    const classes = useStyles(useStyles);
    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');
    const { updateSessionData } = useContext(SessionContext);

    useEffect(()=>{
      setEmail(localStorageService.getItem('email') ?? "");
    }, [])

    const handleLogin = () => {
        //Ensure username and password are of proper standered and the
        if(!submit){
            submitSet(true);
            _database.api("POST", "/auth/Login", {email: email, password, deviceName: navigator.userAgent}).then(result => {
                if(result){
                  if(result.status === 1){
                    updateSessionData(result.data);
                    localStorageService.setItem('email', email);
                    window.location = "/";
                  }else{
                    submitSet(false);
                    _notify.error(result.errorResult);
                  }
                }
                else{
                  submitSet(false);
                  _notify.error("Could not get session. A unexpected error occurred, check your network connection.");
                }
              });
        }

    };

    return (
      <Container className={classes.container}  maxWidth="xs">
          <h2>Login</h2>
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
          <Button className={classes.button} variant="contained" color="primary" onClick={handleLogin}>
              Login
          </Button>
      </Container>
    );
};

export default Login;
