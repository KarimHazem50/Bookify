var exportedCols = [];
var cols = document.querySelectorAll("#DataTable th");
cols.forEach(function (col) {
    if (!col.classList.contains("js-no-export")) {
        exportedCols.push(col);
    }
});
$(function () {
    $("#DataTable").DataTable({
        "columnDefs": [
            { "targets": [-1], "orderable": false }
        ],
        "responsive": true, "lengthChange": false, "autoWidth": false,
        "buttons": [
            {
                extend: "copy",
                exportOptions: {
                    columns: exportedCols
                }
            },
            {
                extend: "csv",
                exportOptions: {
                    columns: exportedCols
                }
            },
            {
                extend: "excel",
                exportOptions: {
                    columns: exportedCols
                }
            },
            {
                extend: "pdf",
                exportOptions: {
                    columns: exportedCols
                },
                customize: function (doc) {
                    doc.content[1].table.widths =
                        Array(doc.content[1].table.body[0].length + 1).join('*').split('');
                },
            },
            {
                extend: "print",
                exportOptions: {
                    columns: exportedCols
                }
            }
        ]
    }).buttons().container().appendTo('#DataTable_wrapper .col-md-6:eq(0)');
});