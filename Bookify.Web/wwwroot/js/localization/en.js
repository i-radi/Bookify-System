var localization = {
    "noBooksFound": "No books were found!",
    "toggleStatusConfirmation": "Are you sure that you need to toggle this item status?",
    "yes": "Yes",
    "no": "No",
    "savedSuccessfully": "Saved successfully!",
    "somethingWrong": "Something went wrong!",
    "goodJob": "Good Job",
    "Oops": "Oops...",
    "ok": "Ok",
    "cancel": "Cancel"
};

var daterangePickerLocale = {
    "applyLabel": "Apply",
    "cancelLabel": "Cancel",
    "fromLabel": "From",
    "toLabel": "To",
    "customRangeLabel": "Custom",
    "weekLabel": "W",
    "daysOfWeek": [
        "Su",
        "Mo",
        "Tu",
        "We",
        "Th",
        "Fr",
        "Sa"
    ],
    "monthNames": [
        "January",
        "February",
        "March",
        "April",
        "May",
        "June",
        "July",
        "August",
        "September",
        "October",
        "November",
        "December"
    ]
}

var daterangePickerRanges = {
    'Today': [moment(), moment()],
    'Yesterday': [moment().subtract(1, 'days'), moment().subtract(1, 'days')],
    'Last 7 Days': [moment().subtract(6, 'days'), moment()],
    'Last 30 Days': [moment().subtract(29, 'days'), moment()],
    'This Month': [moment().startOf('month'), moment().endOf('month')],
    'Last Month': [moment().subtract(1, 'month').startOf('month'), moment().subtract(1, 'month').endOf('month')]
}