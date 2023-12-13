$(document).ready(function () {
    $('.page-link').on('click', function () {
        var btn = $(this);
        var pageNumber = btn.data('page-number');

        if (btn.parent().hasClass('active')) return;

        $('#PageNumber').val(pageNumber);
        $('#Filters').submit();
    });

    $('.js-date-range').daterangepicker({
        showDropdowns: true,
        autoUpdateInput: false,
        autoApply: true,
        minYear: 2020,
        maxDate: new Date(),
        locale: daterangePickerLocale
    });

    $('.js-date-range').on('apply.daterangepicker', function (ev, picker) {
        $(this).val(picker.startDate.format('MM/DD/YYYY') + ' - ' + picker.endDate.format('MM/DD/YYYY'));
    });
});