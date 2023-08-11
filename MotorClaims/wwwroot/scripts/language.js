const handleLanguagePopup = () => {
    const language = document.getElementById('language');
    language.classList.toggle("show");
    // document.getElementById('')
}

const handleLanguage = (language) => {
    // localStorage.setItem('lang', language);
    console.log('lan', language)
    var bodyTags = document.getElementsByTagName("body");
    var bodyTag = bodyTags[0]; // Since getElementsByTagName returns a collection, we access the first element (index 0)

    // Now you can manipulate the bodyTag as desired
    if(language==="A"){
        bodyTag.classList.add('rtl')
    }else{
        bodyTag.classList.add('ltr')
    }
}