$(document).ready(function () {
    $('.js-renew').on('click', function () {

        var subscriberKey = $(this).data('key'); 

        bootbox.confirm({
            message: "Are you sure that you need to renew this subscription?",
            buttons: {
                confirm: {
                    label: 'Yes',
                    className: 'btn-success'
                },
                cancel: {
                    label: 'No',
                    className: 'btn-secondary'
                }
            },
            callback: function (result) {
                if (result) {
                    $.post({
                        url: `/Subscribers/RenewSubscription?sKey=${subscriberKey}`,
                        data: {
                            '__RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
                        },
                        success: function (row) {
                            $('#SubscriptionsTable').find('tbody').append(row);

                            var activeIcon = $('#ActiveStatusIcon');
                            activeIcon.removeClass('d-none');
                            activeIcon.siblings('svg').remove();
                            activeIcon.parents('.card').removeClass('bg-warning').addClass('bg-success');

                            $('#RentalButton').removeClass('d-none');

                            $('#CardStatus').text('Active subscriber');
                            $('#StatusBadge').removeClass('badge-light-warning').addClass('badge-light-success').text('Active subscriber');
                            showSuccessMessage();
                        },
                        error: function () {
                            showErrorMessage();
                        }
                    });
                }
            }
        });
    });

    $('.js-cancel-rental').on('click', function () {

        var btn = $(this);

        bootbox.confirm({
            message: "Are you sure that you need to cancel this rental?",
            buttons: {
                confirm: {
                    label: 'Yes',
                    className: 'btn-danger'
                },
                cancel: {
                    label: 'No',
                    className: 'btn-secondary'
                }
            },
            callback: function (result) {
                if (result) {
                    $.post({
                        url: `/Rentals/MarkAsDeleted/${btn.data('id')}`,
                        data: {
                            '__RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
                        },
                        success: function (result) {
                            btn.parents('tr').remove();

                            if ($('#RentalsTable tbody tr').length === 0) {
                                $('#RentalsTable').fadeOut(function () {
                                    $('#Alert').fadeIn();
                                });
                            }

                            var totalCount = $('#TotalCopies');
                            var currentCount = parseInt(totalCount.text());
                            totalCount.text(currentCount - result);
                        },
                        error: function () {
                            showErrorMessage();
                        }
                    });
                }
            }
        });
    });
});