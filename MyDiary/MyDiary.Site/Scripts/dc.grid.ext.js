function GridExtender(gridId) {
    var me = this;
    this.gridId = gridId;
    this.grid = $("#" + gridId).data("kendoGrid");

    this.GridSettingsRestored = false;
    this.GridPageRestored = false;

    this.HandleGridInitOrGridSettingsChange = function (updateGridHeight) {
        if (!me.GridSettingsRestored) {
            me.GridSettingsRestored = true;

            me.initColumnsOrderAndVisibilityChangeHandlers();
            me.restoreColumnsVisibility();
            me.restoreColumnsOrder();
            me.restoreSortingFiltering();
            if (updateGridHeight) {
                Display.SetupGridHeightUpdate(me.gridId);
            }
            me.updateEmptyGridWidth();

            // Should be called the last because causes page reload
            me.grid.dataSource.read();
        } else {
            me.refreshFilterIcons();
            me.saveSortingFilteringData();
            if (me.GridPageRestored) {
                me.saveCurrentPage();
            }
        }
    };

    this.ResetGridSettings = function (reloadContainerDelegate) {
        window.DcStorage.set(Keys.GetColumnsVisibilityKey(), "");
        window.DcStorage.set(Keys.GetColumnsOrderKey(), "");
        window.DcStorage.set(Keys.GetColumnsSortingKey(), "");
        window.DcStorage.set(Keys.GetColumnsFilteringKey(), "");
        window.DcStorage.set(Keys.GetUserPagePositionKey(), "");

        me.GridSettingsRestored = false;
        
        if (reloadContainerDelegate == undefined) {
            document.location.reload(true);
        } else {
            reloadContainerDelegate();
        }
    };

    this.ExtractDataSourceRequest = function () {
        var dataSource = me.grid.dataSource;
        return dataSource.transport.parameterMap(dataSource._params());
    };

    this.ExtractColumnsState = function () {
        var columnStates = [];
        for (var i = 0; i < me.grid.columns.length; i++) {
            columnStates[i] = {
                title: me.grid.columns[i].title,
                field: me.grid.columns[i].field,
                hidden: me.grid.columns[i].hidden == true,
            };
        }
        return columnStates;
    };

    this.RestoreCurrentPage = function (e) {
        var positionState = UserPagePosition.getState(Keys.GetUserPagePositionKey());
        if (positionState) {
            if (me.grid.dataSource.page() != positionState.page) {
                me.grid.dataSource.page(positionState.page);
                e.preventDefault();
            }
        }
        me.GridPageRestored = true;
    };

    this.IsCurrentPageActivate = function() {
        var positionState = UserPagePosition.getState(Keys.GetUserPagePositionKey());
        if (!positionState) {
            return true;
        }
        if (me.grid.dataSource.page() == positionState.page) {
            return true;
        }
        return false;
    };

    this.updateEmptyGridWidth = function() {
        if ($("div[data-role='grid'] .k-grid-content table tr.k-master-row").size() == 0) {
            $("div[data-role='grid'] .k-grid-content table tbody")
                .append("<tr class='t-no-data'><td colSpan='" + ($("div[data-role='grid'] .k-grid-content table col").size() - 1) + "' /></tr>");
        }
    };

    this.saveColumnsVisibilityData = function (toHide, columnName) {

        var saveVisibilityDataInternal = function (coockieKey, toHideInternal, columnNameInternal) {
            var coockie = window.DcStorage.get(coockieKey);
            var columns = coockie == null ? new Array() : coockie.split("|");
            if (toHideInternal) {
                var idx = $.inArray(columnNameInternal, columns);
                if (idx == -1) {
                    columns.push(columnNameInternal);
                }
            } else {
                columns = $.grep(columns, function (value) {
                    return value != columnNameInternal;
                });
            }
            window.DcStorage.set(coockieKey, columns.join("|"));
        };

        saveVisibilityDataInternal(Keys.GetColumnsVisibilityKey(), toHide, columnName);
        saveVisibilityDataInternal(Keys.GetColumnsVisibilityShownKey(), !toHide, columnName);

    };

    this.restoreColumnsVisibility = function () {
        var coockie = window.DcStorage.get(Keys.GetColumnsVisibilityKey());
        var hiddenColumns = coockie == null ? new Array() : coockie.split("|");
        $.each(hiddenColumns, function (idx, columnName) {
            me.grid.hideColumn(columnName);
        });
        
        var coockieShown = window.DcStorage.get(Keys.GetColumnsVisibilityShownKey());
        var shownColumns = coockieShown == null ? new Array() : coockieShown.split("|");
        $.each(shownColumns, function (idx, columnName) {
            me.grid.showColumn(columnName);
        });

    };

    this.saveCurrentPage = function () {
        UserPagePosition.updateState(Keys.GetUserPagePositionKey(), me.grid.dataSource.page());
    };

    this.saveColumnsOrderData = function (orderEventInfo) {
        var columnsOrder = new Array();

        for (var i = 0; i < orderEventInfo.sender.columns.length; i++) {
            if (i == orderEventInfo.oldIndex) continue;
            if (orderEventInfo.oldIndex < orderEventInfo.newIndex) {
                columnsOrder.push(orderEventInfo.sender.columns[i].field);
                if (i == orderEventInfo.newIndex) {
                    columnsOrder.push(orderEventInfo.column.field);
                }
            } else {
                if (i == orderEventInfo.newIndex) {
                    columnsOrder.push(orderEventInfo.column.field);
                }
                columnsOrder.push(orderEventInfo.sender.columns[i].field);
            }
        }
        window.DcStorage.set(Keys.GetColumnsOrderKey(), columnsOrder.join("|"));
    };

    this.restoreColumnsOrder = function () {
        var coockie = window.DcStorage.get(Keys.GetColumnsOrderKey());
        if (coockie != null) {
            var columnsOrder = coockie.split("|");
            for (var i = 0; i < columnsOrder.length; i++) {
                var columnInfo = me.findColumn(columnsOrder[i]);
                if (columnInfo != null && columnInfo.index != i) {
                    me.grid.reorderColumn(i, columnInfo.column);
                }
            }
        }
    };

    this.findColumn = function (columnName) {
        for (var i = 0; i < me.grid.columns.length; i++) {
            if (me.grid.columns[i].field == columnName)
                return { column: me.grid.columns[i], index: i };
        }
        return null;
    };

    this.saveSortingFilteringData = function () {
        var sortJson = JSON.stringify(me.grid.dataSource._sort);
        var filterJson = JSON.stringify(me.grid.dataSource._filter);

        window.DcStorage.set(Keys.GetColumnsSortingKey(), sortJson);
        window.DcStorage.set(Keys.GetColumnsFilteringKey(), filterJson);
    };

    this.restoreSortingFiltering = function () {

        var filterCoockie = window.DcStorage.get(Keys.GetColumnsFilteringKey());
        if (filterCoockie != null && filterCoockie != "") {
            var filterObj = JSON.parse(filterCoockie);
            me.grid.dataSource._filter = filterObj;
        }
        var sortCoockie = window.DcStorage.get(Keys.GetColumnsSortingKey());
        if (sortCoockie != null && sortCoockie != "") {
            var sortObj = JSON.parse(sortCoockie);
            me.grid.dataSource._sort = sortObj;
        }
    };

    this.refreshFilterIcons = function () {
        // Delete filter icons from all columns
        $("#" + me.grid.element[0].id + " th.k-header").each(function (idx, item) {
            $("span.filter-marker", item).remove();
        });

        // Add filter icon to filtered columns
        var filtersContainer = me.grid.dataSource._filter;
        var columnsMarked = new Array();
        if (filtersContainer != undefined && filtersContainer.filters.length > 0) {
            $.each(filtersContainer.filters, function (idx, filter) {
                if (filter.filters != undefined) {
                    filter = filter.filters[0];
                }
                if ($.inArray(filter.field, columnsMarked) == -1) {
                    var th = $('th[data-field="' + filter.field + '"]');
                    $(th).prepend($('<span class="k-icon k-filter filter-marker"></span>'));
                    columnsMarked.push(filter.field);
                }
            });
        }
    };

    this.initColumnsOrderAndVisibilityChangeHandlers = function () {
        me.grid.unbind("columnHide");
        me.grid.unbind("columnShow");
        me.grid.unbind("columnReorder");

        me.grid.bind("columnHide", function (eventInfo) {
            me.hideColumn(eventInfo);
        });

        me.grid.bind("columnShow", function (eventInfo) {
            me.showColumn(eventInfo);
        });

        me.grid.bind("columnReorder", function (eventInfo) {
            me.reorderColumns(eventInfo);
        });
    };

    this.hideColumn = function (eventInfo) {
        var reloadGrid = false; // reload grid only if column with filter were hidden
        if (me.grid.dataSource._filter) {
            var filtersCount = me.grid.dataSource._filter.filters.length;
            me.grid.dataSource._filter.filters = me.clearFilteringForField(eventInfo.column.field, me.grid.dataSource._filter.filters);
            reloadGrid = filtersCount != me.grid.dataSource._filter.filters.length;

            me.saveSortingFilteringData();
        }
        me.saveColumnsVisibilityData(true, eventInfo.column.field);
        if (reloadGrid) {
            me.grid.dataSource.read();
        }
    };

    this.showColumn = function (eventInfo) {
        me.saveColumnsVisibilityData(false, eventInfo.column.field);
    };

    this.reorderColumns = function (eventInfo) {
        me.saveColumnsOrderData(eventInfo);
    };

    this.clearFilteringForField = function (fieldName, filters) {
        var newFilters = new Array();
        for (var i = 0; i < filters.length; i++) {
            if (!filters[i].field) {
                // a two expressions with "OR" (or any not "AND" logic operator) filter
                if (filters[i].filters[0].field != fieldName) { // it is enough to check just the first field name. The second is the same.
                    newFilters.push(filters[i]);
                }
            } else {
                // a one expression filter
                if (filters[i].field != fieldName) {
                    newFilters.push(filters[i]);
                }
            }
        }
        return newFilters;
    };
}

GridExtender.OnGridColumnMenuInit = function (e) {
    var fm = $("div[data-role='filtermenu']", e.container).data("kendoFilterMenu");
    if (fm == undefined || fm == null) {
        return;
    }
    
    if (fm.type == "string") {
        var dd = $(fm.form[0][0]).data("kendoDropDownList");
        dd.value("contains");
        dd.trigger("change");
        dd = $(fm.form[0][3]).data("kendoDropDownList");
        dd.value("contains");
        dd.trigger("change");
    }
};