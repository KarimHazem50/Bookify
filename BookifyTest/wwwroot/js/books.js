$(document).ready(function () {
    $('#DataTable').DataTable({
        serverSide: true,
        processing: true,
        stateSave: true,
        order: [[1, 'asc']],
        lengthMenu: [6, 15, 30, 50, 100],
        language: {
            processing: '<div class="d-flex justify-content-center text-primary align-items-center dt-spinner"><div class="spinner-border" role="status"></div><span class="text-muted ps-2">Loading...</span></div>',
            searchPlaceholder: "Search by title or author...."
        },
        fnDrawCallback: function () {
            $("input[type='search']").css("width", "220px").focus();
            $("input[type='search']").css("height", "40px");
            $("input[type='search']").css("font-size", "16px");
        },
        ajax: {
            url: "/Books/GetBooks",
            type: "POST"
        },
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
                    return `<div class="symbol symbol-circle symbol-50px overflow-hidden mr-3">
                                                <a href="/Books/Details/${row.id}">
                                                    <div class="symbol-label">
                                                        <img src = "/Images/books/Thumb/${(row.imageName === null ? 'image-placeholder.jpg' : row.imageName)}" alt = "Cover" class="w-100">
                                                    </div>
                                                 </a>
                                             </div>
                                             <div class="d-flex flex-column">
                                                <a href="/Books/Details/${row.id}" class="text-primary font-weight-bolder mb-1">${row.title}</a>
                                                <span>${row.authorName}</span>
                                             </div>`
                }
            },
            { "data": "publisher", "name": "Publisher", "className": "align-middle text-center" },
            {
                "name": "PublishingDate",
                "className": "align-middle text-center",
                "render": function (data, type, row) {
                    return `${moment(row.publishingDate).format('ll')}`
                }
            },
            { "data": "hall", "name": "Hall", "className": "align-middle text-center" },
            { "data": "categoriesNames", "name": "CategoriesNames", "orderable": false, "className": "align-middle text-center" },
            {
                "name": "IsAvailableForRental",
                "className": "align-middle text-center",
                "render": function (data, type, row) {
                    return `<span class="badge badge-${(row.isDeleted ? "danger" : row.isAvailableForRental ? "success" : "warning")} fw-bold h5 py-1 px-2">
                                            ${(row.isDeleted ? "Deleted" : row.isAvailableForRental ? "Available" : "Not available")}
                                            </span>`;
                }
            },
            {
                "name": "IsDeleted",
                "className": "align-middle text-center",
                "render": function (data, type, row) {
                    return `<span class="badge badge-${(row.isDeleted ? "danger" : "success")} fw-bold h5 py-1 px-2">
                                            ${(row.isDeleted ? "Deleted" : "Available")}
                                            </span>`;
                }
            },
            {
                "className": "align-middle text-center",
                "orderable": false,
                "render": function (data, type, row) {
                    return `<td>
                                                <div class="btn-group dropleft">
                                                    <button type="button" class="btn btn-secondary dropdown-toggle" data-toggle="dropdown" aria-expanded="false">
                                                        <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor" class="bi bi-grid-fill" viewBox="0 0 16 16">
                                                            <path d="M1 2.5A1.5 1.5 0 0 1 2.5 1h3A1.5 1.5 0 0 1 7 2.5v3A1.5 1.5 0 0 1 5.5 7h-3A1.5 1.5 0 0 1 1 5.5v-3zm8 0A1.5 1.5 0 0 1 10.5 1h3A1.5 1.5 0 0 1 15 2.5v3A1.5 1.5 0 0 1 13.5 7h-3A1.5 1.5 0 0 1 9 5.5v-3zm-8 8A1.5 1.5 0 0 1 2.5 9h3A1.5 1.5 0 0 1 7 10.5v3A1.5 1.5 0 0 1 5.5 15h-3A1.5 1.5 0 0 1 1 13.5v-3zm8 0A1.5 1.5 0 0 1 10.5 9h3a1.5 1.5 0 0 1 1.5 1.5v3a1.5 1.5 0 0 1-1.5 1.5h-3A1.5 1.5 0 0 1 9 13.5v-3z" />
                                                        </svg>
                                                    </button>
                                                    <div class="dropdown-menu">
                                                        <a href="/Books/Edit/${row.id}" class="dropdown-item">Edit</a>
                                                        <a href="javaScript:;" class="dropdown-item js-status" data-url="/Books/ToggleStatus/${row.id}">Toggle Status</a>
                                                    </div>
                                                </div>
                                            </td>`;
                }
            },

        ],
    });
})