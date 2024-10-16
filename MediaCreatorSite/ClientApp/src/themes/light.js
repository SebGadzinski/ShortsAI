import {createTheme} from '@mui/material/styles';

const lightTheme = createTheme({
    palette: {
        mode: 'light',
        primary: {
            main: '#ffffff', // White
        },
        secondary: {
            main: '#f44336', // Red
        },
        text: {
            primary: '#000000', // Black
        },
        card: {
            primary: '#f44336 !important', // White
        },
    },
    typography: {
        fontFamily: 'Arial, sans-serif', // Set your desired font family
    },
    components: {
        MuiCircularProgress: {
            styleOverrides:{
                colorPrimary: {
                    color: 'black',
                },
            },
        },
        MuiOutlinedInput:{
            styleOverrides:{
                root: {
                    '&:hover fieldset': {
                      borderColor: 'black',
                    },
                    '&.Mui-focused fieldset': {
                      borderColor: 'black',
                    },
                },
            }
        },
        MuiFormLabel: {
            styleOverrides:{
                root: {
                    color: 'black',
                    '&.Mui-focused': {
                        color: 'black',
                    },
                },
            },
        },
        MuiInput: {
            styleOverrides: {
                root: {
                    '& fieldset': {
                        borderColor: 'black'
                    },
                    '&:hover fieldset': {
                        borderColor: 'black'
                    },
                    '&.Mui-focused fieldset': {
                        borderColor: 'black'
                    }
                },
                input: {
                    fontSize: '25px',
                    '&::placeholder': {
                      fontSize: '25px',
                    },
                    [createTheme().breakpoints.down('lg')]: {
                        fontSize: '16px',
                        '&::placeholder': {
                          fontSize: '16px',
                        },
                      },
                    textAlign: 'center',
                    boxShadow: '0 2px 4px rgba(0, 0, 0, 0.2)', // Set your desired box shadow here
                }
            }
        },
        MuiButton: {
            styleOverrides: {
                root: {
                    borderColor: '#00000', // This makes the border color white
                    boxShadow: '0px 3px 5px 2px rgba(0, 0, 0, .3)', // Add your desired box-shadow
                },
            },
        },
    }
});

export default lightTheme;
