function BatchOperationsControl(gridId, selectionEnabled, itemClassName, selectAllDelegate) {
    var me = this;
    this.selectAllDelegate = selectAllDelegate;
    this.GridId = gridId;
    this.ItemsSelector = undefined;
    this.SelectionEnabled = selectionEnabled;
    this.ItemClassName = itemClassName;

    if (selectionEnabled) {
        var selectedItemsCounter = $("#selectedItemsCounter");
        this.ItemsSelector = new CommonSelector(selectedItemsCounter, Keys.GetSelectorKey);
        this.ItemClassName = itemClassName;
    }

    this.InitializeEventHandlers = function () {
        $("table.select_cmds").on("click", "#selectAllOnPage", me.selectAllOnPage);
        $("table.select_cmds").on("click", "#selectAll", me.selectAll);
        $("table.select_cmds").on("click", "#deselectAll", me.DeSelectAll);

        $("#" + me.GridId).on('click', '.' + me.ItemClassName, function (args) { me.ItemsSelector.SelectionHandler(args); });
    };

    this.selectAllOnPage = function () {
        me.ItemsSelector.selectAll(me.ItemClassName);
    };

    this.selectAll = function () {
        if (me.selectAllDelegate == undefined) {
            alert('Для текущей таблицы функция "Выбрать все" не реализована.');
        } else {
            window.showProgressBar();
            me.selectAllDelegate(me.persistSelectAllResultDelegate);
        }
    };

    this.persistSelectAllResultDelegate = function (data) {
        me.ItemsSelector.saveEntitiesInPageVariable(data);
        me.RefreshControls();
    };

    this.RefreshControls = function () {
        if (me.SelectionEnabled) {
            var selectors = $('.' + me.ItemClassName);
            me.ItemsSelector.RefreshControlsState(selectors);
        }
        window.hideProgressBar();
    };

    this.DeSelectAll = function() {
        var checkboxes = $('.' + me.ItemClassName);
        if (me.ItemsSelector != undefined)
            me.ItemsSelector.ClearCheckBoxes(checkboxes);
    };
}