var Keys = {
    GetCurrentTabIndexDelegate: undefined,
    GetCurrentPageIdDelegate: undefined,
    GetColumnsVisibilityKey: function () { return "life_dc_ColumnsVisibilityData" + Keys.getPath(); },
    GetColumnsVisibilityShownKey: function () { return "life_dc_ColumnsVisibilityShownData" + Keys.getPath(); },
    GetColumnsOrderKey: function () { return "life_dc_ColumnsOrderData" + Keys.getPath(); },
    GetColumnsSortingKey: function () { return "life_dc_SortingData" + Keys.getPath(); },
    GetColumnsFilteringKey: function () { return "life_dc_FilteringData" + Keys.getPath(); },
    GetSelectorKey: function (skipPath) {
        if (skipPath != undefined && skipPath != null && skipPath)
            return "life_dc_GridSelector";
        
        return "life_dc_GridSelector" + Keys.getPath();
    },
    GetUserPagePositionKey: function () { return "life_dc_UserPagePosition" + Keys.getPath(); },
    GetCurrenTabKey: function () { return "life_dc_CurrenTab" + Keys.getPath(true); },

    getPath: function (notUseTabIndex) {
        if (Keys.GetCurrentTabIndexDelegate != undefined && (notUseTabIndex == undefined || !notUseTabIndex)) {
            return document.location.href.replace("http://", "").replace(/\//g, "_").replace(/./g, "_") + "_tab" + Keys.GetCurrentTabIndexDelegate();
        }
        else {
            var path = document.location.href.replace("http://", "").replace(/\//g, "_").replace(/./g, "_");
            if (Keys.GetCurrentPageIdDelegate != undefined) {
                path = path + "_pageid" + Keys.GetCurrentPageIdDelegate();
            }
            return path;
        }
    },
    
    GetRandomKey: function() {
        return Math.floor((1 + Math.random()) * 0x10000).toString(16).substring(1);
    }
}