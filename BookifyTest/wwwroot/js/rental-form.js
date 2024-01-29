var CurrentCopies = [];
var selectedCopies = [];

$(document).ready(function () {

    if (document.querySelectorAll('.js-copy').length > 0) {

                                    // Edit Mode //

        HandleMasterDetailsFormforSubmit()
        CurrentCopies = selectedCopies;
        // Handle Remove and ReAdd Copy

        var BtnsRemove = document.querySelectorAll(".js-remove-copy")
        BtnsRemove.forEach(function (BtnRemove) {
            BtnRemove.addEventListener("click", function () {
                /* Update UI */
                BtnRemove.closest(".CopyRow").querySelector("#BookTitle").classList.toggle("text-line-through");
                BtnRemove.closest(".CopyRow").querySelector("#BookTitle").classList.toggle("opacity-50");
                BtnRemove.closest(".CopyRow").querySelector("#DispalyImage").classList.toggle("opacity-50");
                BtnRemove.classList.toggle("btn-outline-danger");
                BtnRemove.classList.toggle("js-remove-copy");
                BtnRemove.classList.toggle("btn-outline-success");
                BtnRemove.classList.toggle("js-ReAdd");
                BtnRemove.textContent == "Remove" ? BtnRemove.textContent = "Re-Add" : BtnRemove.textContent = "Remove";

                /* Hanlde Form for Submit */
                BtnRemove.closest(".CopyRow").querySelector("#HiddenInput").classList.toggle("js-copy")
                BtnRemove.closest(".CopyRow").querySelector("#HiddenInput").toggleAttribute("name")

                HandleMasterDetailsFormforSubmit()

                /* Hanlde Show Buttom for Submit */
                if (JSON.stringify(selectedCopies) !== JSON.stringify(CurrentCopies)) {
                    document.querySelector('#SubmitMasterDetailsForm').classList.add("d-flex");
                    document.querySelector('#SubmitMasterDetailsForm').classList.remove("d-none");
                }
                else {
                    document.querySelector('#SubmitMasterDetailsForm').classList.remove("d-flex");
                    document.querySelector('#SubmitMasterDetailsForm').classList.add("d-none");
                }
            })
        })

   
    }
   

    var buttonSearch = document.querySelector(".js-SubmitSearch");
    var inputSearch = document.querySelector("#Search");

    buttonSearch.addEventListener("click", function (e) {
        if (selectedCopies.find(c => c.serial == inputSearch.value)) {
            showErrorMessage("This copy is already exist")
            e.preventDefault();
        }

        if (selectedCopies.length >= maxAllowedCopies) {
            showErrorMessage(`You can not add more than ${maxAllowedCopies} book(s)`)
            e.preventDefault();
        }
    });
})

function HandleMasterDetailsFormforSubmit() {
    var copies = document.querySelectorAll(".js-copy");
    selectedCopies = []
    copies.forEach(function (input, i) {
        selectedCopies.push({ serial: input.getAttribute("value"), bookId: input.getAttribute("data-book-id") })
        input.setAttribute("name", `selectedCopies[${i}]`)
    })
}

function onAddCopySuccess(copy) {

    // Check can not add more than one copy for the same book
    var bookId = $(copy).find('.js-copy').data('book-id')
    if (selectedCopies.find(c => c.bookId == bookId)) {
        showErrorMessage(`You can not add more than one copy for the same book`);
        return;
    }

    // Add copy to form
    $("#CopiesContiner").prepend(copy)

    // Remove old value from search input 
    document.querySelector("#Search").value = ""

    HandleMasterDetailsFormforSubmit();


    // Handle Show Submit Button
    document.querySelector('#SubmitMasterDetailsForm').classList.remove("d-none");
    document.querySelector('#SubmitMasterDetailsForm').classList.add("d-flex");


    // Handle Submit Button Form
    $('#CopiesContiner').on('submit', function () {
        var btn = document.querySelector(".js-indicator-submit-main-form");
        var btnText = btn.querySelector(".input-btn-form-submit");
        var spinner = btn.querySelector(".js-indicator-spinner");
        btn.setAttribute("disabled", "true");
        oldValueButton = btnText.innerHTML;
        btnText.innerHTML = "Please wait...";
        spinner.classList.add("spinner-border");
    });


    // Handle Remove Copy from Form
    var RemoveCopy = document.querySelector(".js-remove-copy")
    RemoveCopy.addEventListener("click", function (e) {
        var parentElement = e.target.closest(".CopyRow");

        $(parentElement).remove();

        HandleMasterDetailsFormforSubmit();

         // Handle Show Submit Button
        if (selectedCopies.length == 0 || JSON.stringify(selectedCopies) === JSON.stringify(CurrentCopies)) {
            document.querySelector('#SubmitMasterDetailsForm').classList.remove("d-flex");
            document.querySelector('#SubmitMasterDetailsForm').classList.add("d-none");
        }
    })
}
