$(document).ready(function () {
    //date range picker
    var currentDate = new Date();
    var maxDate = new Date(currentDate.getFullYear() - 18, currentDate.getMonth(), currentDate.getDate());
    $('.js-DateRagePicker').daterangepicker({
        singleDatePicker: true,
        showDropdowns: true,
        minYear: 1950,
        maxDate: maxDate,
        drops: "up"
    });
    $('.js-select2').select2({
        placeholder: $(this).attr("placeholder")
    });
    $('.js-select2').on('select2:select', function (e) {
        $('#Form').validate().element(e.target);
    });
    $('.js-select2-Governorate').on('select2:select', function (e) {
        var id = e.target.value;
        $.get({
            url: '/Subscribers/GetAreas/' + id,
            success: function (elements) {
                var areas = document.querySelector('.js-areas');
                areas.innerHTML = "<option value=''></option>";
                elements.forEach(function (option) {
                    let newOption = document.createElement("option");
                    newOption.value = option.id;
                    newOption.text = option.name;
                    areas.appendChild(newOption);
                });
            },
            error: function () {
                showErrorMessage();
            }
        });
    });
});
// Start override image preview
const previewImage = (event) => {
    const imageFiles = event.target.files;
    const imageFilesLength = imageFiles.length;
    if (imageFilesLength > 0) {
        const imageSrc = URL.createObjectURL(imageFiles[0]);
        const imagePreviewElement = document.querySelector("#preview-selected-image");
        imagePreviewElement.src = imageSrc;
        imagePreviewElement.style.display = "block";
        $('#Form').validate().element(event.target);
    }
};
// End override image preview