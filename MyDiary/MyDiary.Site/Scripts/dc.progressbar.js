function showProgressBar() {
    $("#dc-common-progress-bar").show();
}

function hideProgressBar() {
    $("#dc-common-progress-bar").hide();
}

/*
  Use this function for dialog with progress bar
  @dialogElement - (object) dialog object
  @executingFunc - (function) main executing function with progress bar
  @executingSuccessFunc - (function) function delegated in [executingFunc]
  @executingFailFunc -  (function) function delegated in [executingFunc]
  @errorMessage - [string] this text used in the error message if [executingFunc] is fail
*/
function dialogStartWaiting(dialogElement, executingFunc, executingSuccessFunc, executingFailFunc, errorMessage) {
    dialogElement.dialog("destroy");
    showProgressBar();
    var executingSuccessFuncProxi = GetExecutingSuccessProxi(executingSuccessFunc);
    var executingFailFuncProxi = GetExecutingFailProxi(executingFailFunc, errorMessage);
    executingFunc(executingSuccessFuncProxi, executingFailFuncProxi);
}

/*
  Use this function for dialog with progress bar
  @dialogElement - (object) dialog object
  @executingFunc - (function) main executing function with progress bar
  @executingSuccessFunc - (function) function delegated in [executingFunc]
  @executingFailFunc -  (function) function delegated in [executingFunc]
  @errorMessage - [string] this text used in the error message if [executingFunc] is fail
  @parameter - [object] parameter inexecutingFunc
*/
function dialogStartWaitingWithParameter(dialogElement, executingFunc, executingSuccessFunc, executingFailFunc, errorMessage, parameter) {
    dialogElement.dialog("destroy");
    showProgressBar();
    var executingSuccessFuncProxi = GetExecutingSuccessProxi(executingSuccessFunc);
    var executingFailFuncProxi = GetExecutingFailProxi(executingFailFunc, errorMessage);
    executingFunc(executingSuccessFuncProxi, executingFailFuncProxi, parameter);
}

function GetExecutingSuccessProxi(executingSuccessFunc) {
    if(!executingSuccessFunc) {
        return function(data) { hideProgressBar(); };
    }
    return function(data) {
        hideProgressBar();
        executingSuccessFunc(data);
    };
}

function GetExecutingFailProxi(executingFailFunc, errorMessage) {
    if(!executingFailFunc) {
        return function(data) {
            hideProgressBar();
            showErrorMessage(errorMessage);
        };
    }
    return function(data) {
        hideProgressBar();
        executingFailFunc(data);
    };
}

function showErrorMessage(errorMessage) {
    var content = $("<div> </div>");
    if (!errorMessage) {
        errorMessage = 'Извините, операция завершилась неудачей.';
    }
    content.text(errorMessage);
    displayInformationDialog(content, 'ошибка');
}