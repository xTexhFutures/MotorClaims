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
                maxDate: '0',
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

(function ($) {
    $.fn.ForceIntegerEntry = function (AcceptNegative) {

        return this.each(function () {

            $(this).keydown(function (e) {
                var key = e.charCode || e.keyCode || 0;
                if (AcceptNegative) {
                    if (key == 189 || key == 109) {
                        var hasMinus = $(this).val().indexOf("-") > -1;
                        if (hasMinus) {
                            $(this).val($(this).val().replace("-", ""));
                            if ($(this).change) {
                                $(this).change();
                            }
                            return false;
                        } else {
                            $(this).val("-" + $(this).val());
                            if ($(this).change) {
                                $(this).change();
                            }
                            return false;
                        }
                    }
                }
                return (
                    key == 8 ||
                    key == 9 ||
                    key == 46 ||
                    (key >= 37 && key <= 40) ||
                    (key >= 48 && key <= 57) ||
                    (key >= 96 && key <= 105));
            });

            $(this).blur(function (e) {
                if ($(this).val() == "-") {
                    $(this).val("");
                }
                return true;
            });
        });
    };
})(jQuery);


(function ($) {
    $.fn.ForceDecimalNegativeEntry = function (type) {

        return this.each(function () {

            $(this).keydown(function (e) {
                var key = e.charCode || e.keyCode || 0;
            
               
                        if (key == 189 || key == 109) {
                            var hasMinus = $(this).val().indexOf("-") > -1;
                            if (hasMinus) {
                                $(this).val($(this).val().replace("-", ""));
                                if ($(this).change) {
                                    $(this).change();
                                }
                                return false;
                            } else {
                                $(this).val("-" + $(this).val());
                                if ($(this).val() != "-" && $(this).change) {
                                    $(this).change();
                                }
                                return false;
                            }
                        }
                    
                    return (
                        key == 8 ||
                        key == 9 ||
                        key == 46 ||
                        (key >= 37 && key <= 40) ||
                        (key >= 48 && key <= 57) ||
                        (key >= 96 && key <= 105));
                
            });


            $(this).blur(function (e) {
                if ($(this).val() == "-") {
                    $(this).val("");
                }
                $(this).val(FormatDecimal($(this).val()));
                return true;
            });
        });
    };
})(jQuery);

(function ($) {
    $.fn.ForceDecimalEntry = function (type) {

        return this.each(function () {

            $(this).keydown(function (e) {
                var key = e.charCode || e.keyCode || 0;
                if (key == 190 ||
                    key == 110) {
                    if ($(this).val()) {
                        if ($(this).val().indexOf(".") > -1) {
                            return false;
                        } else {
                            return true;
                        }
                    } else {
                        $(this).val("0.");
                        return false;
                    }
                }
                else {
                    return (
                        key == 8 ||
                        key == 9 ||
                        key == 46 ||
                        (key >= 37 && key <= 40) ||
                        (key >= 48 && key <= 57) ||
                        (key >= 96 && key <= 105));
                }
            });
        });
    };
})(jQuery);
(function ($) {
    $.fn.UnForceIntegerEntry = function (type) {
        return this.each(function () {
            $(this).unbind('keydown');
            $(this).unbind('blur');
        });
    };
})(jQuery);
(function ($) {
    $.fn.UnForceDecimalEntry = function (type) {
        return this.each(function () {
            $(this).unbind('keydown');
            $(this).unbind('blur');
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

$('#txtEmail').autocomplete({
    source: function (request, response) {
        $.getJSON("/api/Services/GetUsers/", { name: request.term }, function (data) {
            response($.map(data, function (val, i) {
                return {
                    label: val.email,
                    ReportGroupId: val.userName
                };
            }))
        });
    },
    minLength: 1,
    select: function (event, ui) {
        $('#txtName').val(ui.item.ReportGroupId);
        $('#txtEmail').val(ui.item.label);
    },
    change: function (event, ui) {
        if (!ui.item) {
            $('#txtEmail').val("");
            $('#txtName').val("");
        }
    }
}).focus(function () {
    if ($(this).attr('state') != 'open' && !$('#txtName').val()) {
        $(this).autocomplete("search");
    }
});



$('#txtAssignTo').autocomplete({
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
    minLength: 1,
    select: function (event, ui) {
        $('#txtAssignToId').val(ui.item.ReportGroupId);
    },
    change: function (event, ui) {
        if (!ui.item) {
            $('#txtAssignTo').val("");
            $('#txtAssignToId').val("");
        }
    }
}).focus(function () {
    if ($(this).attr('state') != 'open' && !$('#txtAssignToId').val()) {
        $(this).autocomplete("search");
    }
});





