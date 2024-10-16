const localStorageService = {
    getItem: (key) => {
        try {
            const value = localStorage.getItem(key);
            return value ? JSON.parse(value) : null;
        } catch (error) {
            console.error(`Error retrieving item from local storage: ${error}`);
            return null;
        }
    },

    setItem: (key, value) => {
        try {
            localStorage.setItem(key, JSON.stringify(value));
        } catch (error) {
            console.error(`Error setting item in local storage: ${error}`);
        }
    }
};

export default localStorageService;
