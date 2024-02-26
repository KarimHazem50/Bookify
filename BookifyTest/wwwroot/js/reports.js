$(document).ready(function () {
    $('.js-select2-multi').select2({
        placeholder: $(this).attr("placeholder"),
        closeOnSelect: false,
    });

    $('#DataRangePicker').daterangepicker({
        autoApply: true,
        autoUpdateInput: false,
        showDropdowns: true,
        minYear: 2020,
        maxDate: new Date(),
    });

    $('#DataRangePicker').on('apply.daterangepicker', function (ev, picker) {
        $(this).val(picker.startDate.format('MM/DD/YYYY') + ' - ' + picker.endDate.format('MM/DD/YYYY'));
    });


    document.querySelectorAll(".pagination .page-item").forEach(function (e) {
        e.addEventListener("click", function () {
            if (e.classList.contains("active") || e.classList.contains("disabled")) return;
            document.querySelector("#PageNumber").setAttribute("value", e.querySelector(".page-link").getAttribute("data-index"));
            document.querySelector("#Form").submit();
        })
    });

    document.querySelector(".js-form-books")?.addEventListener('submit', function () {
        disableSubmitButtonForm();
    });
})