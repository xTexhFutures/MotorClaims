

const editMember = (id, body) => {
  const element = document.getElementById(id);
  const editMember = document.getElementById(body)
  element.classList.toggle('active');
  editMember.classList.toggle('active');
}

const deleteMember = (id, body) => {
  const element = document.getElementById(id);
  const deleteMember = document.getElementById(body)

  deleteMember.classList.toggle('active');
}

const addNewMember = () => {
  window.location.href('add-employess-gcc.html')
}

// const handleMemberDropdown = (id) => {
//     console.log('ttt')
//     const element = document.getElementById(id);
//     element.classList.toggle('active');
// }


// add dependent ====================

// Get the modal
var modal = document.getElementById("add-dependent-modal");

// Get the button that opens the modal
var btn = document.getElementById("open-dependent-modal");

// Get the <span> element that closes the modal
var span = document.getElementsByClassName("close")[0];

// When the user clicks the button, open the modal 
//btn.onclick = function () {
//  modal.style.display = "block";
//}

// When the user clicks on <span> (x), close the modal
//span.onclick = function () {
//  modal.style.display = "none";
//}

// When the user clicks anywhere outside of the modal, close it
window.onclick = function (event) {
  if (event.target == modal) {
    modal.style.display = "none";
  }
}

const editDependent = () => {
  var modal = document.getElementById("add-dependent-modal");
  modal.style.display = "block";
}

const closeEdit = () => {
  var modal = document.getElementById("add-dependent-modal");
  modal.style.display = "none";
}

// delete modal ================================

// Get the modal
var modalDeleteDependent = document.getElementById("delete-dependent-modal");

// Get the button that opens the modal
var btnDeleteDependent = document.getElementsByClassName("open-delete-dependent-modal")[0];

// Get the <span> element that closes the modal
var spanDelete = document.getElementsByClassName("close-delete-dependent")[0];

// When the user clicks the button, open the modal 
//btnDeleteDependent.onclick = function () {
//  modalDeleteDependent.style.display = "flex";
//}

// When the user clicks on <span> (x), close the modal
//spanDelete.onclick = function () {
//  modalDeleteDependent.style.display = "none";
//}

// When the user clicks anywhere outside of the modal, close it
window.onclick = function (event) {
  if (event.target == modalDeleteDependent) {
    modalDeleteDependent.style.display = "none";
  }
}


const deleteDependent = () => {
  var modalDeleteDependent = document.getElementById("delete-dependent-modal");
  modalDeleteDependent.classList.remove('display-none');
  modalDeleteDependent.classList.add('display-block');
}

const closeDeleteDependent = () => {
  var modalDeleteDependent = document.getElementById("delete-dependent-modal");
  modalDeleteDependent.classList.remove('display-block');
  modalDeleteDependent.classList.add('display-none');
}

// const deleteItem = ()=>{
//     const deleteM = document.getElementById("delete-m-modal");
//     deleteM.classList.toggle('display-block');
//     // modalDeleteDependent.classList.add('display-block');
// }

// const closeDeleteItem = ()=>{
//     var modalDeleteDependent = document.getElementById("delete-dependent-modal");
//     modalDeleteDependent.classList.remove('display-block');
//     modalDeleteDependent.classList.add('display-none');
// }

// const openDeleteSummary = () => {
//   console.log('openDeleteSummary');
//   // var modalDeleteDependent = document.getElementById("delete-summary-modal");
//   // modalDeleteDependent.classList.remove('display-none');
//   // modalDeleteDependent.classList.add('display-block');
// }
