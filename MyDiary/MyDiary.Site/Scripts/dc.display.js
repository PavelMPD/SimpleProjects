var Display = {
    // Display any value
    value: function (value) {
        if (value != null) {
            return value;
        }
        return '';
    },
    
    // Display date
    date: function (date, format) {
        if (date != null) {
            return kendo.toString(new Date(parseInt(date.replace(/[A-Za-z$--/]/g, ''))), format);
        }
        return '';
    },
    
    // Display currency
    currency: function (val) {
        if (val != null) {
            return parseFloat(val).toFixed(0).toString().replace(/\B(?=(\d{3})+(?!\d))/g, " ");
        }
        else {
            return 0;
        }
    },
    
    scrollTo: function (elemId) {

        setTimeout(function () {
            var container = $(".k-grid-content");
            var elem = $("#scrollAnchor" + elemId);
            if (elem.length == 0) {
                // Posotion to row.
                var selector = $('.debtor-selector[value=' + elemId + ']');
                if (selector.length == 0) {
                    return;
                }
                container.scrollTop(selector.offset().top - container.offset().top + container.scrollTop()-40);
                return;
            }
            //Position to card
            var detailContainer = elem.closest(".k-detail-row");
            var masterContainer = detailContainer.prev();

            var containerTop = container.offset().top;
            var containerBottom = containerTop + container.height();

            var detailContainerTop = detailContainer.offset().top;
            var detailContainerBottom = detailContainerTop + detailContainer.height();

            if (detailContainer.css("display") != "none") {
                if (detailContainerBottom > containerBottom) {
                    container.scrollTop(masterContainer.offset().top - container.offset().top + container.scrollTop());
                } 
            }
        }, 50);
    },
    
    SetupGridHeightUpdate: function (gridName, multiplier) {
        Display.UpdateGridHeight(gridName, multiplier);

        $(window).unbind("resize");
        $(window).resize(function () {
            Display.UpdateGridHeight(gridName, multiplier);
        });

    },

    UpdateGridHeight: function (gridName, multiplier) {
        var winHeight = $(window).height();

        if (winHeight < 400) return;

        var gridContainer = $("#" + gridName);
        var gridHeader = $("thead", gridContainer);
        var headerBottom = 0;
        if (gridHeader.offset()) {
            headerBottom = gridHeader.offset().top + gridHeader.height();
        }

        if (multiplier == undefined) {
            multiplier = 0.07;
        }

        $(".k-grid-content", gridContainer).height(winHeight - headerBottom - winHeight * multiplier);
    },
    
    SetupContainerHeightUpdate: function (containerName, containerTop) {
        Display.UpdateContainerHeight(containerName, containerTop);

        $(window).unbind("resize");
        $(window).resize(function () {
            Display.UpdateContainerHeight(containerName, containerTop);
        });

    },

    UpdateContainerHeight: function (containerName, containerTop) {
        var container = $("#" + containerName);
        
        var winHeight = $(window).height();

        if (winHeight < 400) return;

        if (containerTop == undefined && container.offset() != undefined)
            containerTop = container.offset().top;
        container.height(winHeight - containerTop - containerTop * 0.09);
    }
}