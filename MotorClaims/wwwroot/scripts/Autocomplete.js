(function ($) {
    $.fn.ForceDateEntry = function (type) {
        return this.each(function () {
            $(this).keydown(function (e) {
                var key = e.charCode || e.keyCode || 0;
                if (key == 109 || key == 189) {
                    return false;
                }
                else {
                    return true;
                }
            });
            $(this).keyup(function (e) {
                var key = e.charCode || e.keyCode || 0;

                if (e.target.value.length == 2) {
                    e.target.value = e.target.value + "-";
                }
                if (e.target.value.length == 5) {
                    e.target.value = e.target.value + "-";
                }

            });
            $(this).datepicker({
                changeMonth: true,
                changeYear: true,
                dateFormat: 'dd-mm-yy',
                showAnim: 'blind',
                onClose: function (dateText, instance) {
                    if (dateText) {
                        var date;
                        try {
                            date = $.datepicker.parseDate(instance.settings.dateFormat, dateText, instance.settings);
                        }
                        catch (e) {
                        }
                        if (!date) {
                            $(this).effect("highlight", { color: '#FF9393' });
                            $(this).val("");
                        }
                    }
                }
            });
        });
    };
})(jQuery);

(function ($) {
    $.fn.UnForceDateEntry = function (type) {
        return this.each(function () {
            $(this).unbind('keydown');
            $(this).unbind('keyup');
            try {
                $(this).datepicker("destroy");
            }
            catch (e) { };
        });
    };
})(jQuery);

$('#txtDelegateFrom').autocomplete({
    source: function (request, response) {
        $.getJSON("/api/Services/GetUsers/", { name: request.term }, function (data) {
            response($.map(data, function (val, i) {
   
                return {
                    label: val.userName,
                    ReportGroupId: val.id
                };
            }))
        });
    },
    minLength: 0,
    select: function (event, ui) {
        $('#txthfDelegateFrom').val(ui.item.ReportGroupId);
    },
    change: function (event, ui) {
        if (!ui.item) {
            $('#txtDelegateFrom').val("");
            $('#txthfDelegateFrom').val("");
        }
    }
}).focus(function () {
    if ($(this).attr('state') != 'open' && !$('#txthfDelegateFrom').val()) {
        $(this).autocomplete("search");
    }
});

$('#txtDelegateTo').autocomplete({
    source: function (request, response) {
        $.getJSON("/api/Services/GetUsers/", { name: request.term }, function (data) {
            response($.map(data, function (val, i) {

                return {
                    label: val.userName,
                    ReportGroupId: val.id
                };
            }))
        });
    },
    minLength: 0,
    select: function (event, ui) {
        $('#txthfDelegateTo').val(ui.item.ReportGroupId);
    },
    change: function (event, ui) {
        if (!ui.item) {
            $('#txtDelegateTo').val("");
            $('#txthfDelegateTo').val("");
        }
    }
}).focus(function () {
    if ($(this).attr('state') != 'open' && !$('#txthfDelegateTo').val()) {
        $(this).autocomplete("search");
    }
});


$('#txtApproverEmail').autocomplete({
    source: function (request, response) {
        $.getJSON("/api/Services/GetUsers/", { name: request.term }, function (data) {
            response($.map(data, function (val, i) {

                return {
                    label: val.email,
                    ReportGroupId: val.id
                };
            }))
        });
    },
    minLength: 0,
    select: function (event, ui) {
        $('#txthfApproverEmail').val(ui.item.ReportGroupId);
    },
    change: function (event, ui) {
        if (!ui.item) {
            $('#txtApproverEmail').val("");
            $('#txthfApproverEmail').val("");
        }
    }
}).focus(function () {
    if ($(this).attr('state') != 'open' && !$('#txthfApproverEmail').val()) {
        $(this).autocomplete("search");
    }
});

