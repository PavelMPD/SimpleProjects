var DcStorage = 
    {
        set: function (key, value) {
             $.jStorage.set(key, value);
        },

        get: function (key) {
           return $.jStorage.get(key)
        }
    }