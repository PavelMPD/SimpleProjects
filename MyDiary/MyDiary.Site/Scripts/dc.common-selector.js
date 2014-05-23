function CommonSelector(counterDisplayElement, getSelectorKeyDelegate) {
    var me = this;
    this.displayElement = counterDisplayElement;

    if (getSelectorKeyDelegate == undefined || getSelectorKeyDelegate == null) {
        alert("CommonSelector getSelectorKeyDelegate is required.");
        return;
    }
    this.getSelectorKey = getSelectorKeyDelegate;

    this.getState = function(entityId) {
        var selectedEntities = me.resotreEntities();
        var position = $.inArray(entityId, selectedEntities);
        var state = true;
        if (position == -1) {
            state = false;
        }

        me.displaySelectedEntities();
        return state;
    };

    this.setState = function (entityId, selected) {
        var selectedEntities = me.resotreEntities();
        var position = $.inArray(entityId, selectedEntities);

        if (position == -1 && !selected) {
        }

        if (position == -1 && selected) {
            selectedEntities.push(entityId);
            me.saveEntities(selectedEntities);
        }

        if (position != -1 && selected) {
        }

        if (position != -1 && !selected) {
            selectedEntities.splice(position, 1);
            me.saveEntities(selectedEntities);
        }

        me.displaySelectedEntities();
        return true;
    };

    this.clear = function () {
        me.getPageStorage().remove();
        window.DcStorage.set(this.getSelectorKey(), null);
        me.displaySelectedEntities();
    };

    this.getSelectedEntitiesArray = function() {
        return me.resotreEntities();
    };

    this.getCount = function() {
        var selectedEntities = me.resotreEntities();
        return selectedEntities.length;
    };

    this.displaySelectedEntities = function() {
        if (me.displayElement == null) {
            return;
        }
        this.displayElement.text(me.getCount());
    };

    this.resotreEntities = function () {
        var storedEntities;
        var pageStorage = me.getPageStorage();
        if (pageStorage.length == 0) {
            storedEntities = window.DcStorage.get(me.getSelectorKey());
        } else {
            storedEntities = pageStorage.val();
        }
         
        return storedEntities == null ? new Array() : storedEntities.split("|");
    };

    this.saveEntities = function (array)
    {
        var pageStorage = me.getPageStorage();
        if (pageStorage.length == 0) {
            me.saveEntitiesInCookie(array);
        } else {
            me.saveEntitiesInPageVariable(array);
        }
    },

    this.saveEntitiesInCookie = function (array) {
        // remove page variable storage of selectAll. will store selected in cookie
        me.getPageStorage().remove();

        if (array.length == 0) {
            window.DcStorage.set(me.getSelectorKey(), null);
        } else {
            window.DcStorage.set(me.getSelectorKey(), array.join("|"));
        }
    };

    this.saveEntitiesInPageVariable = function (array) {
        // remove cookie. starting store in page variable
        window.DcStorage.set(me.getSelectorKey(), null);
        if (array.length == 0) {
            me.getPageStorage().val('');
        } else {
            if (me.getPageStorage().length == 0) {
                $("body").append("<input type='hidden' value='' id='" + me.getSelectorKey(false) + "' />");
            }
            me.getPageStorage().val(array.join("|"));
        }
    };

    this.getPageStorage = function () {
        return $("#" + me.getSelectorKey(false));
    },

    this.selectAll = function (itemClassName) {
        // remove page variable storage of selectAll. will store selected in cookie
        me.getPageStorage().remove();
        
        var selector = this;
        $('.' + itemClassName + ':not(:checked)').each(function () {
            var chb = $(this);
            chb.prop('checked', true);
            selector.setState(chb.val(), true);
        });
    };

    // -- Для чекбоксов грида  --

    this.RefreshControlsState = function (controls) {
        for (index in controls) {
            controls[index].checked = me.getState(controls[index].value);
        }
    };

    this.SelectionHandler = function (args) {
        var checkBox = me.extractSelector(args);
        if (!me.setState(checkBox.value, checkBox.checked)) {
            checkBox.checked = !checkBox.checked;
        }
    };

    this.extractSelector = function(args) {
        return args.currentTarget;
    };

    this.ClearCheckBoxes = function(checkBoxes) {
        me.clear();
        me.RefreshControlsState(checkBoxes);
    };
}


