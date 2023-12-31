$(document).ready(function () {
    // Select2
    $('.js-select2').select2({
        placeholder: $(this).attr("placeholder")
    });
    let inputTileBook = document.querySelector('.js-input-title-validate');
    $('.js-select2').on('select2:select', function (e) {
        $('#Form').validate().element(e.target);
        if (inputTileBook.value != "")
            $('#Form').validate().element(inputTileBook);
    });
    inputTileBook.addEventListener("input", function () {
        $('#Form').validate().element(inputTileBook);
        if (document.querySelector('.js-select2').value != "")
            $('#Form').validate().element(document.querySelector('.js-select2'));
    });
    $('.js-select2-multi').select2({
        placeholder: $(this).attr("placeholder"),
        closeOnSelect: false,
    });
    $('.js-select2-multi').on('select2:select', function (e) {
        $('#Form').validate().element(e.target);
    });
    //date range picker
    $('.js-DateRagePicker').daterangepicker({
        singleDatePicker: true,
        showDropdowns: true,
        minYear: 1800,
        maxDate: new Date(),
        drops: "up"
    });
});
// Start override image preview
let btnCanselImage = document.querySelector('.btn-cansel');
btnCanselImage.onclick = function () {

    document.getElementById('preview-selected-image').src = " ";
    btnCanselImage.style.display = "none";
    document.querySelector(".inputForImage").value = null;
}
const previewImage = (event) => {
    const imageFiles = event.target.files;
    const imageFilesLength = imageFiles.length;
    if (imageFilesLength > 0) {
        btnCanselImage.style.display = "flex";
        const imageSrc = URL.createObjectURL(imageFiles[0]);
        const imagePreviewElement = document.querySelector("#preview-selected-image");
        imagePreviewElement.src = imageSrc;
        imagePreviewElement.style.display = "block";
    }
};
document.querySelector(".nav-link").addEventListener('click', function () {
    document.querySelector(".image-preview-container").classList.toggle('hanlde-align-image');
})
// End override image preview