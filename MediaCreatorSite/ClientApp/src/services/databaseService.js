export const _database = {
  /**
   * Sends request in form data format
   * @param {string} type
   * @param {string} path
   * @param {object} data
   * @returns Result Object
   */
  api_form_data: async (type, path, data) => {
    let fetchObj = {
      method: type
    };
    if (data != null) {
      fetchObj.body = data;
    }
    return await fetch(path, fetchObj)
      .then((response) => {
        return response.json();
      })
      .then((result) => {
        return result;
      })
      .catch((error) => {
        console.error(error);
      });
  },

  /**
   * Sends request in JSON format
   * @param {string} type
   * @param {string} path
   * @param {object} data
   * @returns emailSent bool
   */
  api: async (type, path, data = null) => {
    let fetchObj = {
      method: type
    };
    if (data != null) {
      fetchObj.headers = {
        Accept: "application/json",
        "Content-Type": "application/json"
      };
      fetchObj.body = JSON.stringify(data);
    }
    return await fetch(path, fetchObj)
      .then((response) => {
        return response.json();
      })
      .then((result) => {
        if (typeof result === "string") result = JSON.parse(result);

        return result;
      })
      .catch((error) => {
        // Mabye send to logs table as error
        console.error(error);
      });
  },

  downloadVideo: async (data) => {
    if(data == null) return {status: 2, errorResult: 'Invalid Request'}

    let fetchObj = {
      method: 'POST',
      headers: {
        Accept: "application/json",
        "Content-Type": "application/json"
      },
      body: JSON.stringify(data)
    };

    return await fetch('video/Download', fetchObj)
    .then((response) => {
      return response.json(); // Parse JSON response
    })
    .then((result) => {
      if (typeof result === "string") result = JSON.parse(result);
      if(result.exception) return result;
      // Open blob url in new tab
      window.open(result.data, '_blank');
    })
    .catch((error) => {
      console.error(error);
    });
  },

};
