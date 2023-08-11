const selectIdTypes = (id) => {
    // console.log('ite', e.target)
    const element = document.getElementById(id);
    console.log('id', element.id, element.value, element.checked)
    // const value = value;

    if(element.id === 'radio-card-1' && element.value === 'iqama' && element.checked){
        const selectedId = document.getElementById('selected-id');
        selectedId.className='detailed-wrapper iqama-user';
    }

    else if(element.id === 'radio-card-2' && element.value === 'borderNo' && element.checked){
        const selectedId = document.getElementById('selected-id');
        selectedId.className='detailed-wrapper border-user';
    }
    else if(element.id === 'radio-card-3' && element.value === 'gcc' && element.checked){
        const selectedId = document.getElementById('selected-id');
        selectedId.className='detailed-wrapper gcc-user';
    }
    else if(element.id === 'radio-card-4' && element.value === 'bedoon' && element.checked){
        const selectedId = document.getElementById('selected-id');
        selectedId.className='detailed-wrapper bedoon-user';
    }

    else{
        const selectedId = document.getElementById('selected-id');
        selectedId.className='detailed-wrapper relative-user';
    }
}

const selectDependentIdTypes = (id) => {
    // console.log('ite', e.target)
    const element = document.getElementById(id);
    console.log('id', element.id, element.value, element.checked)
    // const value = value;

    if(element.id === 'dependent-id-iqama' && element.value === 'iqama' && element.checked){
        const selectedId = document.getElementById('selected-dependent-id');
        selectedId.className='dependent-selected-member-type iqama-user';
    }

    else if(element.id === 'dependent-id-border' && element.value === 'border' && element.checked){
        const selectedId = document.getElementById('selected-dependent-id');
        selectedId.className='dependent-selected-member-type border-user';
    }
    else if(element.id === 'dependent-id-gcc' && element.value === 'gcc' && element.checked){
        const selectedId = document.getElementById('selected-dependent-id');
        selectedId.className='dependent-selected-member-type gcc-user';
    }
    

    else{
        const selectedId = document.getElementById('selected-dependent-id');
        selectedId.className='dependent-selected-member-type bedoon-user';
    }
}

// saperate dependent add



const selectedIdType =(id)=>{
    // console.log('ite', e.target)
    const element = document.getElementById(id);
    if(element.id === 'radio-card-1' && element.value === 'iqama'){
        element.checked = true;
        const selectedId = document.getElementById('selected-id');
        selectedId.className='detailed-wrapper iqama-user';
    }
    console.log('id', element.id, element.value, element.checked)
    // const value = value;
}

const openDependentPopup = () => {
    const slidePopUp = document.getElementById('slide-popup-dependent')
    slidePopUp.classList.toggle('open');
}

const deletePopup = () => {
    const deletePopup = document.getElementById('delete-popup')
    const deletePopupBackdrop = document.getElementById('delete-backgrop')
    deletePopup.classList.toggle('open');
    deletePopupBackdrop.classList.toggle('active');
}

const selectClass = (id) => {
    var elems = document.querySelectorAll(".coverage-plan-card.active");

    [].forEach.call(elems, function (el) {
        el.className = el.className.replace(/\bactive\b/, "");
    });
    const documentId = document.getElementById(id)
    documentId.classList.add('active');
}

const selectMetitalStatus = (id) => {
    var elems = document.querySelectorAll(".radio-horizontal-wrapper.active");

    [].forEach.call(elems, function (el) {
        el.className = el.className.replace(/\bactive\b/, "");
    });
    const documentId = document.getElementById(id)
    documentId.classList.add('active');
}

const selectGender = (id) => {
    var elems = document.querySelectorAll(".radio-horizontal-wrapper.active");

    [].forEach.call(elems, function (el) {
        el.className = el.className.replace(/\bactive\b/, "");
    });
    const documentId = document.getElementById(id)
    documentId.classList.add('active');
}

const selectDependentType = (id) => {
    var elems = document.querySelectorAll(".radio-horizontal-wrapper.active");

    [].forEach.call(elems, function (el) {
        el.className = el.className.replace(/\bactive\b/, "");
    });
    const documentId = document.getElementById(id)
    documentId.classList.add('active');
}

const selectDependentIDType = (id) => {
    var elems = document.querySelectorAll(".radio-horizontal-wrapper.active");

    [].forEach.call(elems, function (el) {
        el.className = el.className.replace(/\bactive\b/, "");
    });
    const documentId = document.getElementById(id)
    documentId.classList.add('active');
}

const selectDependentGender = (id) => {
    var elems = document.querySelectorAll(".radio-horizontal-wrapper.active");

    [].forEach.call(elems, function (el) {
        el.className = el.className.replace(/\bactive\b/, "");
    });
    const documentId = document.getElementById(id)
    documentId.classList.add('active');
}

const displayDependent = (id, iconDisplay) => {
    const displayDep = document.getElementById(id)
    const displayIcon = document.getElementById(iconDisplay)
    displayDep.classList.toggle('active');
    displayIcon.classList.toggle('active');
}

const memberDetails = () =>{
    const elementId = document.getElementById('member-details')
    const elementId_1 = document.getElementById('member-details-1')
    const elementId_2 = document.getElementById('member-details-2')
    const elementId_3 = document.getElementById('member-details-3')
    const elementId_4 = document.getElementById('member-details-4')
    elementId.classList.toggle('active')
    elementId_1.classList.toggle('active')
    elementId_2.classList.toggle('active')
/*    elementId_3.classList.toggle('active')*/
    //elementId_4.classList.toggle('active')
}

const selectExistingMember = () =>{
    const displayElement = document.getElementById('dependent-content-id');
    displayElement.classList.add('active')
}


// seperated dependent

const selectDependent = (id) => {
    // console.log('ite', e.target)
    
    const element = document.getElementById(id);
    console.log('id', element.id, element.value, element.checked)
    // const value = value;

    if(element.id === 'dependent-spouse' && element.value === 'spouse' && element.checked){
        console.log('spouse');
        const selectedId = document.getElementById('dependent-type-id');
        // const selectedDependentType = document.getElementById('selected-dependent-id');
        selectedId.classList.add('active')
        // selectedDependentType.classList.add('active')
    }

    else if(element.id === 'dependent-child' && element.value === 'child' && element.checked){
        console.log('child')
        // const selectedId = document.getElementById('selected-dependent-id');
        // selectedId.className='dependent-selected-member-type border-user';
    }
    else if(element.id === 'dependent-newborn' && element.value === 'new-born' && element.checked){
        console.log('new-born')
        // const selectedId = document.getElementById('selected-dependent-id');
        // selectedId.className='dependent-selected-member-type gcc-user';
    }
}

const selectDependentSeperateIDType = (id) => {
    // console.log('ite', e.target)
    
    const element = document.getElementById(id);
    console.log('id', element.id, element.value)
    // const value = value;
    if(element.id === 'dependent-id-iqama' && element.value === 'iqama' && element.checked){
        console.log('iqama')
        const selectedDependentType = document.getElementById('dependent-id');
        const addedId = document.getElementById('added-id');
        const personalDetails = document.getElementById('not-id-iqama');
        console.log('dependent-id-border', selectedDependentType);
        selectedDependentType.classList.add('active');
        addedId.classList.add('iqama');
        addedId.classList.remove('border');
        addedId.classList.remove('gcc');
        addedId.classList.remove('badoon');
        personalDetails.classList.remove('active')
    }
    
    else if(element.id === 'dependent-id-border' && element.value === 'border' && element.checked){
        console.log('iqama')
        const personalDetails = document.getElementById('not-id-iqama');
        const selectedDependentType = document.getElementById('dependent-id');
        console.log('dependent-id-border', selectedDependentType);
        selectedDependentType.classList.add('active');
        const addedId = document.getElementById('added-id');
        addedId.classList.remove('iqama');
        addedId.classList.add('border');
        addedId.classList.remove('gcc');
        addedId.classList.remove('badoon');
        personalDetails.classList.remove('active')
    }

    else if(element.id === 'dependent-id-gcc' && element.value === 'gcc' && element.checked){
        const selectedDependentType = document.getElementById('dependent-id');
        const addedId = document.getElementById('added-id');
        
        console.log('dependent-id-border', selectedDependentType);
        selectedDependentType.classList.add('active');
        const idInfo = document.getElementById('id-information');
        const withContact = document.getElementById('not-id-iqama-detailed');
        const merital = document.getElementById('member-extra-merital');
        const personalDetails = document.getElementById('not-id-iqama');
        idInfo.classList.remove('active');
        withContact.classList.add('active');
        merital.classList.add('active');
        merital.classList.remove('not-active')
        personalDetails.classList.add('active')
        const documents = document.getElementById('not-iqama');
        documents.classList.add('active');
        
    }
    else if(element.id === 'dependent-id-bedoon' && element.value === 'bedoon' && element.checked){
        const selectedDependentType = document.getElementById('dependent-id');
        const addedId = document.getElementById('added-id');
        
        console.log('dependent-id-border', selectedDependentType);
        selectedDependentType.classList.add('active');
        addedId.classList.remove('iqama');
        addedId.classList.remove('border');
        addedId.classList.remove('gcc');
        addedId.classList.add('bedoon');
        const personalDetails = document.getElementById('not-id-iqama');
        personalDetails.classList.add('active');
        const documents = document.getElementById('not-iqama');
        documents.classList.add('active');
    }
}

const displayIdDetails = (border) =>{
    const idInfo = document.getElementById('id-information');
    const withContact = document.getElementById('not-id-iqama-detailed');
    const merital = document.getElementById('member-extra-merital');
    const addedId = document.getElementById('added-id');
    
    
    idInfo.classList.add('active');
    withContact.classList.add('active');
    merital.classList.add('not-active');
    if(addedId.classList === "dependnet-form separated-dependent bedoon"){
        const documents = document.getElementById('not-iqama');
        documents.classList.add('active');
    }
    // contact.classList.add('active');
}

