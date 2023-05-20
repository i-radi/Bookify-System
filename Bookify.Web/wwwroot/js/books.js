$(document).ready(function () {
    $('[data-kt-filter="search"]').on('keyup', function () {
        var input = $(this);
        datatable.search(this.value).draw();
    });

    datatable = $('#Books').DataTable({
        serverSide: true,
        processing: true,
        stateSave: true,
        language: {
            processing: '<div class="d-flex justify-content-center text-primary align-items-center dt-spinner"><div class="spinner-border" role="status"><span class="visually-hidden">Loading...</span></div><span class="text-muted ps-2">Loading...</span></div>'
        },
        ajax: {
            url: '/Books/GetBooks',
            type: 'POST'
        },
        'drawCallback': function () {
            KTMenu.createInstances();
        },
        order: [[1, 'asc']],
        columnDefs: [{
            targets: [0],
            visible: false,
            searchable: false
        }],
        columns: [
            { "data": "id", "name": "Id", "className": "d-none" },
            {
                "name": "Title",
                "className": "d-flex align-items-center",
                "render": function (data, type, row) {
                    return `<div class="symbol symbol-50px overflow-hidden me-3">
                                                <a href="/Books/Details/${row.id}">
                                                    <div class="symbol-label h-70px">
                                                        <img src="${(row.imageThumbnailUrl === null ? '/images/books/no-book.jpg' : row.imageThumbnailUrl)}" alt="cover" class="w-100">
                                                    </div>
                                                </a>
                                            </div>
                                            <div class="d-flex flex-column">
                                                <a href="/Books/Details/${row.id}" class="text-primary fw-bolder mb-1">${row.title}</a>
                                                <span>${row.author}</span>
                                            </div>`;
                }
            },
            { "data": "publisher", "name": "Publisher" },
            {
                "name": "PublishingDate",
                "render": function (data, type, row) {
                    return moment(row.publishingDate).format('ll')
                }
            },
            { "data": "hall", "name": "Hall" },
            //{ "data": "categories", "name": "Categories", "orderable": false },
            {
                "name": "IsAvailableForRental",
                "render": function (data, type, row) {
                    return `<span class="badge badge-light-${(row.isAvailableForRental ? 'success' : 'warning')}">
                                                ${(row.isAvailableForRental ? 'Available' : 'Not Available')}
                                            </span>`;
                }
            },
            {
                "name": "IsDeleted",
                "render": function (data, type, row) {
                    return `<span class="badge badge-light-${(row.isDeleted ? 'danger' : 'success')} js-status">
                                                ${(row.isDeleted ? 'Deleted' : 'Available')}
                                            </span>`;
                }
            },
            {
                "className": 'text-end',
                "orderable": false,
                "render": function (data, type, row) {
                    return `<a href="#" class="btn btn-light btn-active-light-primary btn-sm" data-kt-menu-trigger="click" data-kt-menu-placement="bottom-end">
                            Actions
                            <!--begin::Svg Icon | path: icons/duotune/arrows/arr072.svg-->
                            <span class="svg-icon svg-icon-5 m-0">
                                <svg width="24" height="24" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
                                    <path d="M11.4343 12.7344L7.25 8.55005C6.83579 8.13583 6.16421 8.13584 5.75 8.55005C5.33579 8.96426 5.33579 9.63583 5.75 10.05L11.2929 15.5929C11.6834 15.9835 12.3166 15.9835 12.7071 15.5929L18.25 10.05C18.6642 9.63584 18.6642 8.96426 18.25 8.55005C17.8358 8.13584 17.1642 8.13584 16.75 8.55005L12.5657 12.7344C12.2533 13.0468 11.7467 13.0468 11.4343 12.7344Z" fill="currentColor"></path>
                                </svg>
                            </span>
                            <!--end::Svg Icon-->
                        </a>
                                <div class="menu menu-sub menu-sub-dropdown menu-column menu-rounded menu-gray-800 menu-state-bg-light-primary fw-semibold w-200px py-3" data-kt-menu="true" style="">
                            <!--begin::Menu item-->
                            <div class="menu-item px-3">
                                <a href="/Books/Edit/${row.id}" class="menu-link px-3">
                                    Edit
                                </a>
                            </div>
                            <!--end::Menu item-->
                            <!--begin::Menu item-->
                            <div class="menu-item px-3">
                                        <a href="javascript:;" class="menu-link flex-stack px-3 js-toggle-status" data-url="/Books/ToggleStatus/${row.id}">
                                    Toggle Status
                                </a>
                            </div>
                            <!--end::Menu item-->
                        </div>`;
                }
            },
        ]
    });
});