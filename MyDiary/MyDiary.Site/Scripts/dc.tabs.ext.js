function TabStripExtender(tabStripId) {
    var me = this;
    this.tabStripId = tabStripId;
    this.tabStrip = $("#" + tabStripId).data("kendoTabStrip");
    this.TabStripRestored = false;

    this.HandleTabSelection = function() {
        me.TabStripRestored = false;
        me.enableTabs(me.TabStripRestored);

        me.clearTabs();
    };

    this.GetCurrentTabIndex = function() {
        return me.tabStrip.select().index();
    };

    this.RestoreLastSelectedTab = function () {
        var currenTab = window.DcStorage.get(Keys.GetCurrenTabKey());
        var selectedTabIndex = currenTab == null ? 0 : currenTab;
        me.tabStrip.select(selectedTabIndex);
    };

    this.SaveSelectedTab = function () {
        window.DcStorage.set(Keys.GetCurrenTabKey(), me.GetCurrentTabIndex());
        me.TabStripRestored = true;
        me.enableTabs(me.TabStripRestored);
    };

    this.ReloadCurrentTab = function () {
        me.TabStripRestored = false;
        me.enableTabs(me.TabStripRestored);
        me.tabStrip.reload(me.tabStrip.select());
    };

    this.enableTabs = function (enable) {
        $.each(me.tabStrip.tabGroup[0].children, function (index) {
            me.tabStrip.enable(me.tabStrip.tabGroup.children("li:eq(" + index + ")"), enable); // disable tab 1
        });
    };

    this.clearTabs = function () {
        $.each(me.tabStrip.tabGroup[0].children, function (index) {
            $("div#" + me.tabStripId + "-" + (index + 1) + ".k-content").html("");
        });
    };
}