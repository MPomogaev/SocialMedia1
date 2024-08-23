var deleteButton = document.getElementById("deleteButton")
var deleteCheckbox = document.getElementById("deleteCheckbox")
var photoInput = document.getElementById("photoInput")
var accountPhoto = document.getElementById("accountPhoto")

deleteButton.addEventListener("click", () => {
    deleteCheckbox.checked = true
    accountPhoto.src = null
    photoInput.value = null
}) 

photoInput.addEventListener("input", () => {
    if (photoInput.files.length == 0) {
        return
    }
    deleteCheckbox.checked = false
    var fr = new FileReader()
    fr.onload = () => {
        accountPhoto.src = fr.result;
    }
    fr.readAsDataURL(photoInput.files[0])
})