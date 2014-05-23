
var UserPagePosition = {
    setState: function(cookieName, userPagePositionInformation) {
        if (userPagePositionInformation) {
            window.DcStorage.set(cookieName, JSON.stringify(userPagePositionInformation));
        } else {
            window.DcStorage.set(cookieName, null);
        }
    },
    getState: function (cookieName) {
        var state = window.DcStorage.get(cookieName);
        if (state) {
            return JSON.parse(state);
        }
        return null;
    },
    updateState: function(cookieName, page, focusItemId) {
        var userPagePositionInformation;
        var state = window.DcStorage.get(cookieName);
        if (state) {
            userPagePositionInformation = JSON.parse(state);

            if (focusItemId != undefined) {
                userPagePositionInformation.focusItemId = focusItemId;
            }
            if (page != undefined) {
                userPagePositionInformation.page = page;
            }
        } else {
            if (focusItemId == undefined) {
                focusItemId = "";
            }
            if (page == undefined) {
                page = 0;
            }

            userPagePositionInformation = new UserPagePositionInformation(page, focusItemId);
        }
        this.setState(cookieName, userPagePositionInformation);
    }
};

function UserPagePositionInformation(page, focusItemId) {
    this.page = page;
    this.focusItemId = focusItemId;
};