
function Alert(message, messageType) {
    $.toast({
        heading: messageType,
        text: message,
        position: 'top-right',
        stack: 55,
        allowToastClose: true,
        icon: messageType.toLowerCase(),
        hideAfter :false
    })
}

function InfoAlert(message) {
    Alert(message, "");
}

function SuccessAlert(message) {
    Alert(message, "Success");
}

function ErrorAlert(message) {
    Alert(message, 'Error');
}
