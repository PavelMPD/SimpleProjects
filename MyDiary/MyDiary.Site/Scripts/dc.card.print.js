var Card = {
    // files: [DebtCollection.Site.ViewModels.FileInfo]
    printDownladLinks: function(files) {
        var resultOutput = '';
        for (var i = 0; i < files.length; i++) {
            resultOutput += '<div>' + printLink('..\\File\\Get\\' + files[i].Id, files[i].DisplayName, '') + '</div>';
        }
        return resultOutput;
    },

    // claims: Array of [DebtCollection.Site.ViewModels.DebtorInfo.ClaimInfo]
    printClaims: function(claims) {
        var outputHtml = '';
        for (var i = 0; i < claims.length; i++) {
            outputHtml += printClaim(claims[i]);
            if (i + 1 < claims.length) {
                outputHtml += $('<dt style="margin-top:15px;"></dt><dd></dd>').outerHTML();
            }
        }
        return outputHtml;
    },
    
    printDebtAnountHistoryGrid: function (historyData) {
        var div = $("<div />");
        div.kendoGrid({
            dataSource: {
                data: historyData,
            },
            scrollable: false,
            columns: [
                {
                    field: "AmountBefore",
                    title: "Сумма до",
                    template: '#= Display.currency(AmountBefore) #',
                    width: 140
                },
                {
                    field: "AmountAfter",
                    title: "Сумма после",
                    template: '#= Display.currency(AmountAfter) #',
                    width: 140
                },                
                {
                    field: "ChangeDate",
                    title: "Дата изменения",
                    template: '#= Display.date(ChangeDate, "'+STANDARD_DATE_FORMAT+'") #',
                    width: 130
                }
            ]
        });
        return div.html();
    }
};

// claim: [DebtCollection.Site.ViewModels.DebtorInfo.ClaimInfo]
function printClaim(claim) {
    var outputHtml = $('<dt> Дата, когда выставить претензию: </dt>').outerHTML()
                    + $('<dd></dd>').text(Display.date(claim.ClaimWhenPutDate, STANDARD_DATE_FORMAT)).outerHTML();

    outputHtml += $('<dt> Дата выставления: </dt>').outerHTML() + $('<dd></dd>').text(Display.date(claim.ClaimDate, STANDARD_DATE_FORMAT)).outerHTML();
    outputHtml += $('<dt> Регистрационный номер:</dt>').outerHTML() + $('<dd></dd>').text(Display.value(claim.ClaimRegistrationNumber)).outerHTML();
    outputHtml += $('<dt> Дата оплаты по претензии:</dt>').outerHTML()
                  + $('<dd></dd>').text(Display.date(claim.ClaimOfClaimPayment, STANDARD_DATE_FORMAT)).outerHTML();
    outputHtml += $('<dt> Дата, до которой оплатить:</dt>').outerHTML()
                    + $('<dd></dd>').text(Display.date(claim.ClaimNecessaryToPay, STANDARD_DATE_FORMAT)).outerHTML();
    outputHtml += $('<dt> Ссылка на документ: </dt>').outerHTML();
    if (claim.File) {
        outputHtml += $('<dd></dd>').html(printLink('..\\File\\Get\\' + claim.File.Id, claim.File.DisplayName, '')).outerHTML();
    } else {
        outputHtml += $('<dd></dd>').outerHTML();
    }
    return outputHtml;
}

function printLink(path, text, styleClass) {
    var link = $('<a></a>');
    link.attr('href', path);
    link.text(text);
    link.addClass(styleClass);
    return link.outerHTML();
}

function showGeneratingStatus(debtorId, documentType, message, showButton, showMessage) {
    var btnNewDocument = $("#subscriber-"+documentType+"-genetrate_" + debtorId);
    var statusDisplayer = $("#subscriber-" + documentType + "-status_" + debtorId);

    statusDisplayer.text(message);
    
    if (showMessage) {
        statusDisplayer.show();
    }
    else {
        statusDisplayer.hide();
    }

    if (showButton) {
        btnNewDocument.show();
    }
    else {
        btnNewDocument.hide();
    }
}

//ToDo: refactor this
function saveCallback(debtorsIds, data, reloadGrid) {

    // Claims - invalid data
    if (data.InvalidDate) {
        window.showClaimsParameterDialog(debtorsIds, data.Message);
        return;
    }
    
    if (data.IsSuccess) {
        window.showMultiOperationDialog("Операция запущена успешнo", false);
    } else {
        if (data.Message == undefined || data.Message == null || data.Message == "")
            data.Message = "Ошибка при запуске операции";
        window.showMultiOperationDialog(data.Message, true);
    }
    
    if (window.Page && window.Page.BatchOperations) {
        window.Page.BatchOperations.DeSelectAll();
    }

    if (reloadGrid != undefined && reloadGrid && window.Page && window.Page.Grid) {
        window.Page.Grid.Refresh();
    }
}
