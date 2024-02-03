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
                            if (document.querySelector(".js-add-rental").classList.contains("d-none")) {
                                document.querySelector(".js-add-rental").classList.remove("d-none")
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


    var btnsCansel = document.querySelectorAll('.js-cansel-rental');
    btnsCansel?.forEach(function (btnCansel) {
        btnCansel.addEventListener("click", function (e) {
            var rental = e.target.closest('tr');
            bootbox.confirm({
                message: "Are you sure that you need to cansel this rental?",
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
                            url: `/Rentals/MarkedAsDeleted/${btnCansel.getAttribute('data-id')}`,
                            data: {
                                "__RequestVerificationToken": document.querySelector("input[name='__RequestVerificationToken']").getAttribute("value")
                            },
                            success: function () {
                                $(rental).remove();

                                var table = document.querySelector('#RentalsTable');
                                var alert = document.querySelector('#AlertNotRentals');

                                if (table.querySelector('tbody').childElementCount == 0) {
                                    table.classList.add("d-none");
                                    alert.classList.remove('d-none');
                                    alert.classList.add('d-flex');
                                }

                                var numberOfCopiesCanseled = parseInt(rental.getAttribute("data-copies"));
                                var oldCopiesCount = parseInt(document.querySelector("#DisplayNumberOfCopies").innerHTML);
                                document.querySelector("#DisplayNumberOfCopies").innerHTML = oldCopiesCount - numberOfCopiesCanseled;
                            },
                            error: function () {
                                showErrorMessage();
                            }
                        });
                    }
                }
            });

        })
    })

});