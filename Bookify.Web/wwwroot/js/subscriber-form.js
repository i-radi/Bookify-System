$(document).ready(function () {
    $('#GovernorateId').on('change', function () {
        var governorateId = $(this).val();
        var areaList = $('#AreaId');

        areaList.empty();
        areaList.append('<option></option>');

        if (governorateId !== '') {
            $.ajax({
                url: '/Subscribers/GetAreas?governorateId=' + governorateId,
                success: function (areas) {
                    $.each(areas, function (i, area) {
                        var item = $('<option></option>').attr("value", area.value).text(area.text);
                        areaList.append(item);
                    });
                },
                error: function () {
                    showErrorMessage();
                }
            });
        }
    });
});