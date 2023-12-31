/* Start Handle Toggle Status */
document.addEventListener("click", function (btn) {
    if (btn.target.classList.contains("js-status-copy")) {
        bootbox.confirm({
            message: "Are you sure that you need to toggle this item status?",
            buttons: {
                confirm: {
                    label: 'Yes',
                    className: 'btn-danger',
                },
                cancel: {
                    label: 'No',
                    className: 'btn-secondary'
                }
            },
            callback: function (result) {
                if (result) {
                    $.post({
                        url: btn.target.getAttribute("data-url"),
                        data: {
                            "__RequestVerificationToken": document.querySelector("input[name='__RequestVerificationToken']").getAttribute("value")
                        },
                        success: function (newElement) {
                            var tr = btn.target.closest("tr");
                            tr.classList.add("flash");
                            setTimeout(function () {
                                $(tr).replaceWith(newElement);
                            }, 800)
                            showSuccessMessage();
                        },
                        error: function () {
                            showErrorMessage();
                        }
                    });
                }
            }
        });
    }
});
/* End Handle Toggle Status */
$(document).ready(function () {
    var bookCopiesCount = document.getElementById("bookCopiesCount").innerText;
    setTimeout(function () {
        let demo = new countUp.CountUp('CopiesCount', bookCopiesCount);
        demo.start();
    }, 1000)
});
function onAddCopySuccess(item) {
    $('#modal-default').modal('hide');
    showSuccessMessage();
    $("tbody").prepend(item);
    var count = document.querySelector(".js-conut-copies").textContent;
    document.querySelector(".js-conut-copies").textContent = parseInt(count) + 1;

    if (parseInt(count) == 0) {
        document.querySelector(".alert").classList.add("d-none");
        document.querySelector(".table").classList.remove("d-none");
    }
}
function onEditCopySuccess(item) {
    $('#modal-default').modal('hide');
    $(Updated).replaceWith(item);
    showSuccessMessage();
}