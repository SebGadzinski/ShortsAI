import React, {createContext, useState} from 'react';

const BaseModalContext = createContext();

const BaseModalProvider = ({children}) => {
    const [open, setOpen] = useState(false);
    const [loading, setLoading] = useState(false);
    const [content, setContent] = useState('');
    const [size, setSize] = useState('xs');

    const toggle = () => {
        if(open) setLoading(loading);
        setOpen(!open);
    };

    const show = (isLoading = false) =>{
        setLoading(isLoading);
        setOpen(true);
    }

    const load = (isLoading = true) =>{
        setLoading(isLoading);
    }

    const hide = () =>{
        setLoading(false);
        setOpen(false);
    }

    return (
        <BaseModalContext.Provider value={
            {toggle, show, load, hide, loading, open, content, setContent, size, setSize}
        }>
            {children} </BaseModalContext.Provider>
    );
};

export {
    BaseModalContext,
    BaseModalProvider
};
