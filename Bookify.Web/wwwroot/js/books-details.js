function onAddCopySuccess(row) {
    showSuccessMessage();
    $('#Modal').modal('hide');

    $('tbody').prepend(row);
    KTMenu.createInstances();

    var count = $('#CopiesCount');
    var newCount = parseInt(count.text()) + 1;
    count.text(newCount);

    $('.js-alert').addClass('d-none');
    $('table').removeClass('d-none');
}

function onEditCopySuccess(row) {
    showSuccessMessage();
    $('#Modal').modal('hide');

    $(updatedRow).replaceWith(row);
    KTMenu.createInstances();
}