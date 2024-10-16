import React from 'react';
import { CircularProgress, Typography } from '@mui/material';

const Loader = ({ size = 24, message = 'Loading...' }) => {
  return (
    <div style={{ display: 'flex', flexDirection:'column', width: 'fit-content', alignItems: 'center' }}>
      <CircularProgress size={size} />
      <br></br>
      <Typography style={{ display: 'block'}}>
        {message}
      </Typography>
    </div>
  );
};

export default Loader;
