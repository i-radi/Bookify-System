var localization = {
    "noBooksFound": "لم يتم العثور على أي كتب!",
    "toggleStatusConfirmation": "هل أنت متأكد من تغيير حالة هذا العنصر؟",
    "yes": "نعم",
    "no": "لا",
    "savedSuccessfully": "تم الحفظ بنجاح!",
    "somethingWrong": "حدث خطأ!",
    "goodJob": "عمل جيد",
    "Oops": "تحذير",
    "ok": "تم",
    "cancel": "إلغاء"
};

var daterangePickerLocale = {
    "applyLabel": "تم",
    "cancelLabel": "إلغاء",
    "fromLabel": "من",
    "toLabel": "إلى",
    "customRangeLabel": "محدد",
    "weekLabel": "أ",
    "daysOfWeek": [
        "ح",
        "إ",
        "ث",
        "أ",
        "خ",
        "ج",
        "س"
    ],
    "monthNames": [
        "يناير",
        "فبراير",
        "مارس",
        "إبريل",
        "مايو",
        "يونيو",
        "يوليو",
        "أغسطس",
        "سبتمبر",
        "أكتوبر",
        "نوفمبر",
        "ديسمبر"
    ]
}

var daterangePickerRanges = {
    'اليوم': [moment(), moment()],
    'الأمس': [moment().subtract(1, 'days'), moment().subtract(1, 'days')],
    'أخر 7 أيام': [moment().subtract(6, 'days'), moment()],
    'أخر 30 يوم': [moment().subtract(29, 'days'), moment()],
    'الشهر الحالي': [moment().startOf('month'), moment().endOf('month')],
    'الشهر الماضي': [moment().subtract(1, 'month').startOf('month'), moment().subtract(1, 'month').endOf('month')]
}