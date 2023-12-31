$(document).ready(function () {
    var btn = document.querySelector(".js-renew");
    btn?.addEventListener('click', function (e) {
        var subscriberKey = e.target.getAttribute("data-key");
        bootbox.confirm({
            message: "Are you sure that you need to renew this subscription?",
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
                        url: `/Subscribers/RenewSubscription?sKey=${subscriberKey}`,
                        data: {
                            "__RequestVerificationToken": document.querySelector("input[name='__RequestVerificationToken']").getAttribute("value")
                        },
                        success: function (row) {
                            $('#SubscriptionsTable').find('tbody').append(row);

                            var activeIcon = document.querySelector("#ActiveStatusIcon");
                            var badge = document.querySelector(".badge-status");
                            if (activeIcon.classList.contains('d-none')) {
                                activeIcon.classList.remove("d-none");
                                document.querySelector("#InActiveStatusIcon").remove();
                                activeIcon.closest(".card").classList.remove('bg-warning')
                                activeIcon.closest(".card").classList.add("bg-success");
                                document.querySelector(".card-text").textContent = "Active subscriber";
                                badge.textContent = "Active subscriber";
                                badge.classList.remove("badge-warning");
                                badge.classList.add("badge-success");
                            }

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
});