import {createTheme} from '@mui/material/styles';

const darkTheme = createTheme({
    palette: {
        mode: 'dark',
        primary: {
            main: '#212121', // Dark gray
        },
        secondary: {
            main: '#f44336', // Red
        },
        text: {
            primary: '#ffffff', // White
        },
        card: {
            primary: 'gray !important',
        },
    },
    typography: {
        fontFamily: 'Arial, sans-serif', // Set your desired font family
    },
    components: {
        MuiInput: {
            styleOverrides: {
                root: {
                    '& fieldset': {
                        borderColor: 'white'
                    },
                    '&:hover fieldset': {
                        borderColor: 'white'
                    },
                    '&.Mui-focused fieldset': {
                        borderColor: 'white'
                    }
                },
                input: {
                    fontSize: '25px !important',
                    '&::placeholder': {
                      fontSize: '25px !important',
                    },
                    [createTheme().breakpoints.down('lg')]: {
                        fontSize: '16px !important',
                        '&::placeholder': {
                          fontSize: '16px !important',
                        },
                      },
                    textAlign: 'center',
                    boxShadow: '0 2px 4px rgba(255, 255, 255, 0.2)', // Set your desired box shadow here
                }
            }
        },
        MuiCircularProgress: {
            styleOverrides:{
                colorPrimary: {
                    color: 'white',
                },
            },
        },
        MuiFormLabel: {
            styleOverrides:{
                root: {
                    color: 'white',
                    '&.Mui-focused': {
                        color: 'white',
                    },
                },
            },
        },
        MuiButton: {
            styleOverrides: {
                root: {
                    borderColor: '#ffffff', // This makes the border color white
                    boxShadow: '0px 3px 5px 2px rgba(255, 105, 135, .3)', // Add your desired box-shadow
                },
            },
        },
        MuiCssBaseline: {
            styleOverrides: {
              body: {
                backgroundColor: '#212121 !important', // Dark gray
              },
            },
        },
    }
});

export default darkTheme;
