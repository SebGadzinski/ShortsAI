import { toast } from 'react-toastify';
import $ from 'jquery';
const onceToastComplete = (func) => {
    let sendWhenDone = setInterval(() => {
        if($(".Toastify").find("*").length === 0){
            func();
            clearInterval(sendWhenDone);
        }
    }, 1000);
}
export const _notify = {
    success: (message, time = 5000, afterToastFunc) => {
        toast.success(message, {
            position: "top-center",
            autoClose: time,
            hideProgressBar: false,
            closeOnClick: false,
            pauseOnHover: true,
            draggable: true,
            progress: undefined,
        });
        if(afterToastFunc) onceToastComplete(afterToastFunc);
    },
    error: (message, time = 5000, afterToastFunc) => {
        toast.error(message, {
            position: "top-center",
            autoClose: time,
            hideProgressBar: false,
            closeOnClick: false,
            pauseOnHover: true,
            draggable: true,
            progress: undefined,
        });
        if(afterToastFunc) onceToastComplete(afterToastFunc);
    },
    warning: (message, time = 5000, afterToastFunc) => {
        toast.warn(message, {
            position: "top-center",
            autoClose: time,
            hideProgressBar: false,
            closeOnClick: false,
            pauseOnHover: true,
            draggable: true,
            progress: undefined,
        });
        if(afterToastFunc) onceToastComplete(afterToastFunc);
    },
    info: (message, time = 5000, afterToastFunc) => {
        toast.info(message, {
            position: "top-center",
            autoClose: time,
            hideProgressBar: false,
            closeOnClick: false,
            pauseOnHover: true,
            draggable: true,
            progress: undefined,
        });
        if(afterToastFunc) onceToastComplete(afterToastFunc);
    },
}
