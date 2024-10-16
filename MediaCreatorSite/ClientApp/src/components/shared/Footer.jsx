import React from "react";
import Typography from "@mui/material/Typography";
import Box from "@mui/material/Box";

const Footer = () => {
  return (
    <Box
      component="footer"
      sx={{
        py: 4,
        textAlign: "center"
      }}
    >
      <Typography variant="body1" color="textSecondary">
        Thank you for visiting our website!
      </Typography>
      <Typography variant="body2" color="textSecondary" mt={1}>
        {new Date().getFullYear()} Shorts AI® . All rights reserved.
      </Typography>
      <Typography variant="body2" color="textSecondary" mt={1}>
        Gadzy Software & Consulting
      </Typography>
    </Box>
  );
};

export default Footer;
