const memberAccordion = (id) => {
    element = document.getElementById(id);
    element.classList.toggle("active");
    element.parentElement.classList.toggle("active")
}

const memberAccordion1 = (id) => {
    $('#' + id).toggle();
}

const deleteItem = () => {
    const deleteM = document.getElementById("delete-m-modal");
    deleteM.classList.toggle('display-block');
}

function displaySummaryDependent(id) {
    const ele = document.getElementById(id);
    ele.classList.toggle('display-block');
}

const handleMemberDropdown = (id) => {
    console.log('ttt')
    const element = document.getElementById(id);
    element.classList.toggle('active');
}

const openHealthPopup = (selectedQuestion) => {
    let currentPage = document.getElementById('modal-health-id');
    let element = document.getElementById('health-m-modal')
    switch (selectedQuestion) {
        case 'q1':
            element.classList.toggle('active');
            currentPage.classList.toggle(selectedQuestion);
            return;
        case 'q2':
            element.classList.toggle('active');
            currentPage.classList.toggle(selectedQuestion);
            return;
        case 'q3':
            element.classList.toggle('active');
            currentPage.classList.toggle(selectedQuestion);
            return;
        case 'q4':
            element.classList.toggle('active');
            currentPage.classList.toggle(selectedQuestion);
            return;
        case 'q5':
            element.classList.toggle('active');
            currentPage.classList.toggle(selectedQuestion);
            return;
        case 'q6':
            element.classList.toggle('active');
            currentPage.classList.toggle(selectedQuestion);
            return;
        case 'q7':
            element.classList.toggle('active');
            currentPage.classList.toggle(selectedQuestion);
            return;
        case 'q8':
            element.classList.toggle('active');
            currentPage.classList.toggle(selectedQuestion);
            return;
        default:
            return;
    }
}
function CloseDialogV(id) {
    let element = document.getElementById(id);
    //element.classList.remove('display-block');
    //element.classList.add('display-none');
    element.classList.toggle('active');
}
const closeHealthModal = () => {
    let element = document.getElementById('health-m-modal')
    let currentPage = document.getElementById('add-member-detail');
    element.classList.toggle('active');
    currentPage.className = 'health-lower-part';
}

const CloseDraftMemberAddition = () => {
    let element = document.getElementById('DraftMemberAddition')
    element.classList.toggle('active');
}
function CloseDialog() {

    let element = document.getElementById('health-m-modal')
    let currentPage = document.getElementById('add-member-detail');
    element.classList.toggle('active');
    //currentPage.className = 'health-lower-part';
    window.location = "/Issuance/HealthDeclaration";
}
function CloseEditeMemberDialog() {

    let element = document.getElementById('edit-member-id-popup')

    element.classList.toggle('active');
    //currentPage.className = 'health-lower-part';

}
function CloseApprovalHistory() {

    let element = document.getElementById('details-popup')

    element.classList.toggle('active');
    //currentPage.className = 'health-lower-part';

}
function changeMemberId(id, i, p, n) {
    const element = document.getElementById(id);
    const popupEditMemberId = document.getElementById('edit-member-id-popup')
    element.classList.toggle('active');
    popupEditMemberId.classList.toggle('active');
    $('#edit-member-id-popup').load('/PolicyDetail/EditMember', { Member: i, Policy: p, NationalId: n });
}
function deleteMemberEndors(id, n, p) {

    const element = document.getElementById(id);
    const popupEditMemberId = document.getElementById('delet-member-popup')
    //element.classList.toggle('active');
    popupEditMemberId.classList.toggle('active');
    $('#delet-member-popup').load('/PolicyDetail/DeleteActiveMember', { NationalId: n, policyid: p });
}

function ConfirmQuotation() {
    var modalDeleteDependent = document.getElementById("delete-summary-modal");
    modalDeleteDependent.classList.remove('display-none');
    modalDeleteDependent.classList.add('display-block');
}

function DeleteQuotation() {
    var modalDeleteDependent = document.getElementById("delete-summary-modal10");
    modalDeleteDependent.classList.remove('display-none');
    modalDeleteDependent.classList.add('display-block');
}

function CreditLimit() {
    var modalDeleteDependent = document.getElementById("delete-summary-modal11");
    modalDeleteDependent.classList.remove('display-none');
    modalDeleteDependent.classList.add('display-block');
}
function PremiumSummaryMessage(id) {
    const element = document.getElementById(id);
    element.classList.toggle('active');


}
function DraftMemberAddition(Doc, t, ty) {

    const element = document.getElementById('DraftMemberAddition');
    element.classList.toggle('active');
    $('#DraftMemberAddition').load('/PolicyDetail/DraftMemberAddition', { DocumentId: Doc, type: t, Origin: ty });
}
function ApprovalActions(id, i) {
    const element = document.getElementById(id);
    const popupEditMemberId = document.getElementById('Approval-popup')
    element.classList.toggle('active');
    popupEditMemberId.classList.toggle('active');
    $('#Approval-popup').load('/Approvals/SetApproval', { Id: i });
}
function ApprovalOpsActions(id, i) {
    const element = document.getElementById(id);
    const popupEditMemberId = document.getElementById('Approval-popup')
    element.classList.toggle('active');
    popupEditMemberId.classList.toggle('active');
    $('#Approval-popup').load('/Approvals/SetApprovalOps', { Id: i });
}
function ApprovalFinanceActions(id, i) {
    debugger;
    const element = document.getElementById('Fin-Approval-popup');
    element.classList.toggle('active');
    $('#Fin-Approval-popup').load('/Approvals/SetFinApproval', { Id: i });
}

function LoadHistiryDetails(id) {

    const element = document.getElementById('details-popup')
    element.classList.toggle('active');

    $('#details-popup').load('/Approvals/FilterApprovalsHistory', { Id: id });
}

function LoadCreditLimit(id) {

    const element = document.getElementById('details-popup')
    element.classList.toggle('active');

    $('#details-popup').load('/Managing/EditCreditLimit', { Id: id });
}
function LoadCreditLimit(id) {
    const element = document.getElementById('details-popup')
    element.classList.toggle('active');

    $('#details-popup').load('/Managing/EditCreditLimit', { Id: id });
}
function OpenUWDialog(id) {

    const element = document.getElementById('UWApproval-modal');
    element.classList.toggle('active');
    $('#UWApproval-modal').load('/Approvals/UWApprovalDetails/' + id);
}
function UWActions(id, i) {
    const element = document.getElementById(id);
    const popupEditMemberId = document.getElementById('UWApproval-popup')
    element.classList.toggle('active');
    popupEditMemberId.classList.toggle('active');
    $('#UWApproval-popup').load('/Approvals/SetUWApproval', { Id: i });
}

const paymentMethod = (id) => {
    let sadadElement = document.getElementById('sadad-payment');
    let cardElement = document.getElementById('card-payment');
    let bankElement = document.getElementById('bank-payment');
    let sadadEle = document.getElementById(id);
    let bankEle = document.getElementById(id);
    if (id === 'sadad' && sadadEle.checked) {
        sadadElement.style.display = 'block'
        cardElement.style.display = 'none'
        bankElement.style.display = 'none'
    }
    else if (id === 'bank' && bankEle.checked) {
        bankElement.style.display = 'block'
        cardElement.style.display = 'none'
        sadadElement.style.display = 'none'
    }
    else {
        cardElement.style.display = 'block'
        sadadElement.style.display = 'none'
        bankElement.style.display = 'none'
    }
}

const motorDropdown = (id) => {
    console.log('test')
    const ele = document.getElementById(id);
    ele.classList.toggle('active');
}


// motor ----
const openPpdatePlateNo = () => {
    let element = document.getElementById('update-plate-m-modal')
    element.classList.toggle('active');
}

const closePpdatePlateNo = () => {
    let element = document.getElementById('update-plate-m-modal')
    element.classList.toggle('active');
}
const CloseHealthDialog = (id) => {
    let element = document.getElementById(id)
    element.classList.toggle('active');
    window.location = '/';
}

const openSerialNo = () => {
    let element = document.getElementById('update-serial-m-modal')
    element.classList.toggle('active');
}

const closeSerialNo = () => {
    let element = document.getElementById('update-serial-m-modal')
    element.classList.toggle('active');
}

const openDriverDetail = () => {
    let element = document.getElementById('update-driver-m-modal')
    element.classList.toggle('active');
}

const closeDriverDetail = () => {
    let element = document.getElementById('update-driver-m-modal')
    element.classList.toggle('active');
}

const openDeleteVehicle = () => {
    let element = document.getElementById('delete-vehicle-m-modal')
    element.classList.toggle('active');
}

const closeDeleteVehicle = () => {
    let element = document.getElementById('delete-vehicle-m-modal')
    element.classList.toggle('active');
}

const openAddVehicle = () => {
    let element = document.getElementById('add-vehicle-m-modal')
    element.classList.toggle('active');
}

const closeAddVehicle = () => {
    let element = document.getElementById('add-vehicle-m-modal')
    element.classList.toggle('active');
}

// plate No
const openPlateNo = () => {
    let element = document.getElementById('update-plate-no')
    element.classList.toggle('active');
}

const closePlateNo = () => {
    let element = document.getElementById('update-plate-no')
    element.classList.toggle('active');

}



//var zyllemMain = (function () {
//    function processSubmitLoader() {
//        $('button[data-spinning-button]').click(function () {
//            debugger;
//            var $this = $(this)
//            let formId = $this.data('spinning-button')
//            let id = $this.closest('form').attr('id')
//            let $form = formId ? $('#' + formId) : $this.parents('form')

//            if ($form.length) {

//                //form.valid() will be applicable If you are using jQuery validate https://jqueryvalidation.org/
//                //asp.net mvc used it by default with jQuery Unobtrusive Validation
//           /*     $('#' + id).validate({*/
//                    if($form.valid()) {
//                    $this
//                        .html("<i class='fa fa-circle-o-notch fa-spin'></i>")
//                        .attr('disabled', '')
//                    $form.submit()
//                }
//                    //submitHandler: function (form) {

//                    //    $this
//                    //        .html("<i class='fa fa-circle-o-notch fa-spin'></i>")
//                    //        .attr('disabled', '')

//                    //    $form.submit()

//                    //}
//           /*     });*/
//            }
//        }
//        })
//}
//    return {
//    initSpinnerButton: processSubmitLoader,
//}
//}) ()

//$(document).ready(function () {
//    zyllemMain.initSpinnerButton()
//})





function GenerateQuotation(PolicyId) {

    $('#PrintReport').attr('disabled', true);
    //$('#lblGenerateQuote').show();
    //$('#timerLabel').text("00:10");
    spinner.show();
    $.get("/Issuance/GenerateEskaQuotation/" + PolicyId, function (data) {
        debugger;
        if (data.length > 0) {
            ErrorAlert(data);
        } else {
            $('#PrintReport1').attr('disabled', false);
            $('#PrintReport2').attr('disabled', false);
        }

        spinner.hide();
    });
    /*    setTimeout("countdown()", 120);*/
}
function GenerateEskaQuotation(Id, PId) {

    spinner.show();
    $.post("/Issuance/PrintEskaQuote/" + Id, { PolicyId: PId }, function (data) {
        if (data != null) {
            debugger;
            //download(data, makeid(15));
            var ReportWindow = window.open(data, "window 1", "location=no,menubar=no,status=no,titlebar=no,toolbar=no,resizable=yes");
            ReportWindow.focus();
            ReportWindow.blur();
            /*            window.location.href = "/Issuance/PrintEskaQuote/?id=" + data;*/
            spinner.hide();
            //window.open(data, "_blank");
        }
    });
    /*setTimeout("countdown()", 120);*/
}


function countdown() {
    time = document.getElementById("timerLabel").innerHTML;
    timeArray = time.split(':')
    seconds = timeToSeconds(timeArray);
    if (seconds == '') {
        temp = document.getElementById('timerLabel');
        temp.innerHTML = "00:00";

        $('#PrintReport1').attr('disabled', false);
        $('#PrintReport2').attr('disabled', false);
        $('#lblGenerateQuote').hide();
        return;
    }
    seconds--;
    temp = document.getElementById('countdown');
    document.getElementById("timerLabel").innerHTML = secondsToTime(seconds);
    timeoutMyOswego = setTimeout(countdown, 1000);
}
function timeToSeconds(timeArray) {
    var minutes = (timeArray[0] * 1);
    var seconds = (minutes * 60) + (timeArray[1] * 1);
    return seconds;
}
function secondsToTime(secs) {
    var hours = Math.floor(secs / (60 * 60));
    hours = hours < 10 ? '0' + hours : hours;
    var divisor_for_minutes = secs % (60 * 60);
    var minutes = Math.floor(divisor_for_minutes / 60);
    minutes = minutes < 10 ? '0' + minutes : minutes;
    var divisor_for_seconds = divisor_for_minutes % 60;
    var seconds = Math.ceil(divisor_for_seconds);
    seconds = seconds < 10 ? '0' + seconds : seconds;
    return minutes + ':' + seconds;
}


const openDeleteSummary = (Id, t, PId, originType) => {
    var modalDeleteDependent = document.getElementById("delete-summary-modal");
    modalDeleteDependent.classList.remove('display-none');
    modalDeleteDependent.classList.add('display-block');
    $('#delete-summary-modal').load('/Issuance/LoadDeleteMember/', { id: Id, type: t, PolicyId: PId, OType: originType });
}

const ShowTerms = () => {
    var modalDeleteDependent = document.getElementById("Terms-modal");
    modalDeleteDependent.classList.remove('display-none');
    modalDeleteDependent.classList.add('display-block');
}
function ShowNotes(q9, id) {
    debugger;
    var modalDeleteDependent = document.getElementById(id);
    var q9value = document.getElementById(q9);
    if (q9value.checked) {
        $('#note').show();
    }
    else {
        $('#note').hide();
    }

}
const closeDeleteSummary = () => {
    var modalDeleteDependent = document.getElementById("delete-summary-modal");
    modalDeleteDependent.classList.remove('display-block');
}
const Close = (id) => {
    var model = document.getElementById(id);
    model.classList.remove('display-block');
    model.classList.toggle('active');
}

function DeleteDraftMember(Id, policy, Otype) {
    spinner.show();
    $.post('/Issuance/DeleteDraftMember/', { Id: Id, PolicyId: policy }, function (data) {
        window.location = "/PolicyDetail/RequestDetails/" + policy + "?type=" + Otype;
        closeDeleteSummary();
        spinner.hide();
    });

}
function ShowPolicyHoldetToast(message, header, isSuccess) {
    debugger;
    if (isSuccess) {
        toastr.success(message, header, {
            closeButton: true,
        });
    }
    else {
        toastr.error(
            message,
            header
        );
    }

}
function OpenLogout() {
    var modalDeleteDependent = document.getElementById("delete-summary-modal2");
    modalDeleteDependent.classList.remove('display-none');
    modalDeleteDependent.classList.add('display-block');
}


function ChangeLogin(f) {
    $.post("/Home/ChangeLogin/" + f, function () {
        window.location = "/";
    });
}
//addEventListener("submit", (event) => {
//    debugger;
//    <text>
//        <div className='spinner-wrapper'>
//            <span className="loader"></span>
//        </div>
//</text>


//});

// Motor


function UpdateDocument(Id) {
    const element = document.getElementById('edit-Document-popup');
    element.classList.toggle('active');
    if (Id > 0) {
        $('#edit-Document-popup').load('/Documents/UpdateDocument/' + Id);
    } else {
        $('#edit-Document-popup').load('/Documents/UpdateDocument/');
    }
}

function UpdateAuthority(Id) {
    const element = document.getElementById('edit-Document-popup');
    element.classList.toggle('active');
    if (Id > 0) {
        $('#edit-Document-popup').load('/Setup/UpdateAuthority/' + Id);
    } else {
        $('#edit-Document-popup').load('/Setup/UpdateAuthority/');
    }
}
function UpdateDelegation(Id) {

    const element = document.getElementById('edit-Delegation-popup');
    element.classList.toggle('active');
    if (Id > 0) {
        $('#edit-Delegation-popup').load('/Delegation/UpdateDelegation/' + Id);
    } else {
        $('#edit-Delegation-popup').load('/Delegation/UpdateDelegation/');
    }
}

function UpdateWorkFlow(Id) {
    let element = document.getElementById('edit-WorkFlow-popup')
    element.classList.toggle('active');

    if (Id > 0) {
        $('#edit-WorkFlow-popup').load('/WorkFlow/UpdateWorkFlow/' + Id);
    } else {
        $('#edit-WorkFlow-popup').load('/WorkFlow/UpdateWorkFlow/' + 0);
    }
}

function UpdateFraudIndicator(Id) {
    let element = document.getElementById('edit-FraudIndicator-popup')
    element.classList.toggle('active');

    if (Id > 0) {
        $('#edit-FraudIndicator-popup').load('/Fraud/UpdateFraudIndicator/' + Id);
    } else {
        $('#edit-FraudIndicator-popup').load('/Fraud/UpdateFraudIndicator/');
    }
}

function UpdateWorkFlowApprover(Id) {
    let element = document.getElementById('edit-WorkFlowApprovers-popup')
    element.classList.toggle('active');

    if (Id > 0) {
        $('#edit-WorkFlowApprovers-popup').load('/WorkFlow/UpdateWorkFlowApproversV/' + Id);
    } else {
        $('#edit-WorkFlowApprovers-popup').load('/WorkFlow/UpdateWorkFlowApproversV/'+0);
    }
}
function LoadFraudSetup(Id) {
    let element = document.getElementById('edit-FraudSetup-popup')
    element.classList.toggle('active');

    if (Id > 0) {
        $('#edit-FraudSetup-popup').load('/Fraud/UpdateFraudSetup/' + Id);
    } else {
        $('#edit-FraudSetup-popup').load('/Fraud/UpdateFraudSetup/');
    }
}


function openDeleteDelegation(Id) {
    debugger;
    let element = document.getElementById('delete-Confirmation');
    element.classList.remove('display-none');
    element.classList.add('display-block');
    element.classList.toggle('active');
    $('#delete-Confirmation').load('/Delegation/DeleteDelegation/' + Id);
}

function openDeleteDocument(Id) {
    let element = document.getElementById('delete-Confirmation');
    element.classList.remove('display-none');
    element.classList.add('display-block');
    element.classList.toggle('active');
    $('#delete-Confirmation').load('/Documents/DeleteDocument/' + Id);
}
function openDeleteFraud(Id) {
    let element = document.getElementById('delete-Confirmation');
    element.classList.remove('display-none');
    element.classList.add('display-block');
    element.classList.toggle('active');
    $('#delete-Confirmation').load('/Fraud/DeleteFraud/' + Id);
}

function openDeleteApprover(Id) {
    let element = document.getElementById('delete-Confirmation');
    element.classList.remove('display-none');
    element.classList.add('display-block');
    element.classList.toggle('active');
    $('#delete-Confirmation').load('/WorkFlow/DeleteApprover/' + Id);
}

function UpdateWorkFlowApprovers(Id) {
    let element = document.getElementById('ManageApproversPopup');
    element.classList.remove('display-none');
    element.classList.add('display-block');
    element.classList.toggle('active');
    $('#ManageApproversPopup').load('/WorkFlow/LoadWorkFlowApprovers/' + Id);
}

function LoadClaimantsList(Id, Policy, Vehicle) {

    $('#ClaimantsList').load('/Claims/ClaimantsList/', { Id:Id,PolicyId:Policy,VehicleId:Vehicle });
}

function LoadDocumentsList(ModuleId, ClaimId, Policy, Vehicle, ClaimantId,Reference) {

    $('#ClaimantsDocumentsUpload').load('/Claims/DocumentsUpload/', { ModuleId: ModuleId, ClaimId: ClaimId, PolicyId: Policy, VehicleId: Vehicle, ClaimantId: ClaimantId, Reference: Reference });
}
function LoadClaimantsReserveList(ModuleId, ClaimId, Policy, Vehicle, ClaimantId, Reference) {

    $('#ClaimantsReserve').load('/Claims/ClaimantReserve/', { ClaimantId: ClaimantId, ClaimId: ClaimId });
}

function LoadClaimantsRecoveryList(ModuleId, ClaimId, Policy, Vehicle, ClaimantId, Reference) {

    $('#ClaimantsRecovery').load('/Claims/ClaimantRecovery/', { ClaimantId: ClaimantId, ClaimId: ClaimId });
}
function SurveyorAssign(ClaimId, ClaimantId) {

    let element = document.getElementById('SurveyorAssign');
    element.classList.remove('display-none');
    element.classList.add('display-block');
    element.classList.toggle('active');
    $('#SurveyorAssign').load('/Surveyor/SurveyorAssign/', { ClaimId: ClaimId, ClaimantId: ClaimantId });
}
function SurveyorActions(ClaimId, ClaimantId) {
    debugger;
    let element = document.getElementById('SurveyorActions');
    element.classList.remove('display-none');
    element.classList.add('display-block');
    element.classList.toggle('active');
    $('#SurveyorActions').load('/Surveyor/SurveyorActions/', { ClaimId: ClaimId, ClaimantId: ClaimantId });
}
function OperationAssign(ClaimId,ClaimantId) {
    const element = document.getElementById('OperationAssign');
    element.classList.toggle('active');
    $('#OperationAssign').load('/Operations/OperationAssign/', { ClaimId: ClaimId, ClaimantId: ClaimantId });
}

function TowingLetter(ClaimId, ClaimantId) {
    const element = document.getElementById('TowingLetter');
    element.classList.toggle('active');
    $('#TowingLetter').load('/Towing/TowingLetter/', { ClaimId: ClaimId, ClaimantId: ClaimantId });
}



function OperationReOpen(ClaimId) {
    const element = document.getElementById('OperationReOpen');
    element.classList.toggle('active');
    $('#OperationReOpen').load('/Operations/OperationReOpen/', { ClaimId: ClaimId });
}




function SurveyorReserve(ClaimId) {
    let element = document.getElementById('UpdateReserve');
    element.classList.remove('display-none');
    element.classList.add('display-block');
    element.classList.toggle('active');
    $('#UpdateReserve').load('/Surveyor/UpdateReserve/', { ClaimId: ClaimId });
}

function OperationReserve(ClaimId) {
    let element = document.getElementById('UpdateReserve');
    element.classList.remove('display-none');
    element.classList.add('display-block');
    element.classList.toggle('active');
    $('#UpdateReserve').load('/Operations/UpdateReserve/', { ClaimId: ClaimId });
}

function OperationRecovery(ClaimId) {
    let element = document.getElementById('UpdateRecovery');
    element.classList.remove('display-none');
    element.classList.add('display-block');
    element.classList.toggle('active');
    $('#UpdateRecovery').load('/Operations/UpdateRecovery/', { ClaimId: ClaimId });
}
function LoadPhotos(Id)
{
    let element = document.getElementById('Photos');
    element.classList.remove('display-none');
    element.classList.add('display-block');
    element.classList.toggle('active');
    $('#Photos').load('/Claims/Photos/'+ Id );
}

