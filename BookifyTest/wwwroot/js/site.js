var Updated = "No";
$(document).ready(function () {
    //tinyMCE
    if ($('.js-tinyMCE').length > 0) {
        tinymce.init({
            selector: 'textarea.js-tinyMCE',
            height: 345,
            width: 500,
            init_instance_callback: function (editor) {
                editor.on('input', function () {
                    document.querySelector('.js-tinyMCE').value = tinymce.get('Description').getContent();
                    $('form').not('#SignOut').validate().element(document.querySelector('.js-tinyMCE'));
                });
            },
        });
    }
    // handle local storage for theme
    if (window.localStorage.getItem("theme")) {
        document.documentElement.classList.add("dark-mode");
        HandleActiveClass(darkIconMode?.closest("a"), "#fff");
        if (mainIconMode !== null && darkIconMode !== null)
            mainIconMode.innerHTML = darkIconMode.innerHTML;
        if ($('.js-tinyMCE').length > 0) {
            HandleThemeDarktinyMCE();
        }
    }
    // Handle Submit Button Form
    $('form').not('#SignOut').on('submit', function () {
        var isValid = $(this).valid();
        if (isValid)
            disableSubmitButtonForm();
    });
    // Handle signout
    document.querySelector(".js-signout")?.addEventListener('click', function () {
       $('#SignOut').submit();
    })
});
/* Start Handle Dark Mode */
// Select Icons
let mainIconMode = document.querySelector("span.main-icon-mode");
let darkIconMode = document.querySelector("span.dark-icon-mode");
let lightIconMode = document.querySelector("span.light-icon-mode");
function HandleActiveClass(element, color) {
    document.querySelectorAll(".handle-menu-item").forEach(function (ele) {
        ele.classList.remove("mode-active");
        ele.style.color = color;
    });
    element?.classList.add("mode-active");
}
document.addEventListener("click", function (e) {
    if (e.target.closest("a")?.classList.contains("handle-menu-item")) {
        if (e.target.closest("a").getAttribute("data-mode") === "dark") {
            document.documentElement.classList.add("dark-mode");
            HandleActiveClass(e.target.closest("a"), "#fff")
            mainIconMode.innerHTML = darkIconMode.innerHTML;
            window.localStorage.setItem("theme", "dark-mode");
            if ($('.js-tinyMCE').length > 0) {
                HandleThemeDarktinyMCE();
            }
        }
        else if (e.target.closest("a").getAttribute("data-mode") === "light") {
            document.documentElement.classList.remove("dark-mode");
            HandleActiveClass(e.target.closest("a"), "#333")
            mainIconMode.innerHTML = lightIconMode.innerHTML;
            window.localStorage.removeItem("theme");
            if ($('.js-tinyMCE').length > 0) {
                HandleThemeLighttinyMCE();
            }
        }
    }
});
//tinyMCE
function HandleThemeLighttinyMCE() {
    tinymce.remove(".js-tinyMCE");
    tinymce.init({
        selector: 'textarea.js-tinyMCE',
        height: 345,
        width: 500,
        init_instance_callback: function (editor) {
            editor.on('input', function () {
                document.querySelector('.js-tinyMCE').value = tinymce.get('Description').getContent();
                $('form').not('#SignOut').validate().element(document.querySelector('.js-tinyMCE'));
            });
        },
    });
}
function HandleThemeDarktinyMCE() {
    tinymce.remove(".js-tinyMCE");
    tinymce.init({
        selector: 'textarea.js-tinyMCE',
        height: 345,
        content_css: 'tinymce-5-dark',
        skin: 'tinymce-5-dark',
        width: 500,
        content_style: `
	        .mce-content-body[data-mce-placeholder]:not(.mce-visualblocks)::before {
		      color: #b9b5b5d1;
	        }
	    `,
        init_instance_callback: function (editor) {
            editor.on('input', function () {
                document.querySelector('.js-tinyMCE').value = tinymce.get('Description').getContent();
                $('form').not('#SignOut').validate().element(document.querySelector('.js-tinyMCE'));
            });
        },
    });
}
/* End Handle Dark Mode */
/* Start SweetAlert*/
function showSuccessMessage() {
    Swal.fire({
        icon: 'success',
        title: 'Good',
        text: 'saved successfully',
    })
}
function showErrorMessage(message) {
    var textMessage = 'Something went wrong!';
    Swal.fire({
        icon: 'error',
        title: 'Oops...',
        text: (message.responseText == "" || message.responseText == undefined) ? textMessage : message.responseText
    })
}
/* End SweetAlert*/
/* Start Handle Modal */
let modalHeader = document.querySelector('.modal-title')
let modalBody = document.querySelector('.modal-body')
document.addEventListener('click', function (e) {
    if (e.target.classList.contains("js-render-modal")) {
        $('#modal-default').modal('show');
        modalHeader.textContent = e.target.getAttribute("data-title")
        $.get({
            url: e.target.getAttribute("data-url"),
            success: function (form) {
                modalBody.innerHTML = form;
                $.validator.unobtrusive.parse(modalBody)
                if (e.target.getAttribute("data-edit") === "Yes") {
                    Updated = e.target.closest("tr");
                }
                $('.js-select2-roles').select2({
                    placeholder: $(this).attr("placeholder"),
                });
                $('.js-select2-roles').on('select2:select', function (e) {
                    $('#Form-User').validate().element(e.target);
                });
            },
            error: function () {
                showErrorMessage();
            }
        });
    }
});
var oldValueButton = "";
function disableSubmitButtonForm() {
    var btn = document.querySelector(".js-indicator");
    var btnText = btn.querySelector(".input-btn-form-submit");
    var spinner = btn.querySelector(".js-indicator-spinner");
    btn.setAttribute("disabled", "true");
    oldValueButton = btnText.innerHTML;
    btnText.innerHTML = "Please wait...";
    spinner.classList.add("spinner-border");
}
function allowSubmitButtonForm() {
    var btn = document.querySelector(".js-indicator");
    var btnText = btn.querySelector(".input-btn-form-submit");
    var spinner = btn.querySelector(".js-indicator-spinner");
    btn.removeAttribute("disabled");
    btnText.innerHTML = oldValueButton;
    oldValueButton = "";
    spinner.classList.remove("spinner-border");
}
function onModalBegin() {
    disableSubmitButtonForm();
}
function onModalSuccess(item) {
    $('#modal-default').modal('hide');
    showSuccessMessage();
    var newRow = $(item).addClass("flash");
    if (Updated != "No") {
        $("#DataTable").DataTable().row(Updated).remove().draw(false);
        Updated = "No";
    }
    $("#DataTable").DataTable().row.add(newRow).draw(false);
    setTimeout(function () {
        newRow.removeClass("flash")
    }, 1000)
}
function onModalComplete() {
    allowSubmitButtonForm();
}
/* End Handle Modal */
/* Start Handle Toggle Status */
document.addEventListener("click", function (btn) {
    if (btn.target.classList.contains("js-status")) {
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
                            tr.classList.add("flash")
                            $("#DataTable").DataTable().row(tr).remove().draw(false);
                            var newRow = $(newElement).addClass("flash");
                            $("#DataTable").DataTable().row.add(newRow).draw(false);
                            setTimeout(function () {
                                newRow.removeClass("flash")
                            }, 1000)
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
/* Start Handle Lock Out */
document.addEventListener("click", function (btn) {
    if (btn.target.classList.contains("js-confirm")) {
        bootbox.confirm({
            message: btn.target.getAttribute("data-message"),
            buttons: {
                confirm: {
                    label: 'Yes',
                    className: 'btn-success',
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
                            tr.classList.add("flash")
                            $("#DataTable").DataTable().row(tr).remove().draw(false);
                            var newRow = $(newElement).addClass("flash");
                            $("#DataTable").DataTable().row.add(newRow).draw(false);
                            setTimeout(function () {
                                newRow.removeClass("flash")
                            }, 1000)
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
/* End Handle Lock Out */
