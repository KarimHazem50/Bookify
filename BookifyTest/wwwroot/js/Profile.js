// Start override image preview
const imageProfileLeft = document.querySelector('.profile-user-img');
const imagePreviewElement = document.querySelector("#preview-selected-image");
const defaultAvatarSource = '/assets/images/avatar.png';
let btnCanselImage = document.querySelector('.btn-cansel');

if (imagePreviewElement.src.match(defaultAvatarSource) != null) {
    btnCanselImage.style.display = "none";
}

const previewImage = (event) => {
    const imageFiles = event.target.files;
    const imageFilesLength = imageFiles.length;
    if (imageFilesLength > 0) {
        const imageSrc = URL.createObjectURL(imageFiles[0]);
        imagePreviewElement.src = imageSrc;
        imageProfileLeft.src = imageSrc;
        btnCanselImage.style.display = "flex";
        document.querySelector('#CheckDeleted').value = false;
    }
};

btnCanselImage.onclick = function () {
    imagePreviewElement.src = defaultAvatarSource;
    imageProfileLeft.src = defaultAvatarSource;
    btnCanselImage.style.display = "none";
    document.querySelector(".inputForImage").value = null;
    document.querySelector('#CheckDeleted').value = true;
}
document.querySelector(".nav-link").addEventListener('click', function () {
    document.querySelector(".image-preview-container").classList.toggle('hanlde-align-image');
})
// End override image preview