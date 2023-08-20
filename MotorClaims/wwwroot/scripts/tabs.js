const selectTabs = (tab, tabContent) => {
	let allTabs = document.querySelectorAll(".tab-custom");
	let allTabsContent = document.querySelectorAll(".tabs-one-content");
	// let tabItem = document.getElementById(tab);
	// let tabContentItem = document.getElementById(tabContent);
	// tabContentItem.classList.add('active');
	for (let i = 0; allTabs.length<i; i++) {
		if (allTabs[i].id !== tab) {
			allTabs[i].classList.remove('active');
		} else {
			allTabs[i].classList.add('active');
		}
		if (allTabsContent[i].id !== tabContent) {
			allTabsContent[i].classList.remove('active');
		} else {
			allTabsContent[i].classList.add('active');
		}
	}
}

const selectTabsTwo = (tab, tabContent) => {
	let allTabs = document.querySelectorAll(".tab-custom-two");
	let allTabsContent = document.querySelectorAll(".tabs-one-content-two");
	// let tabItem = document.getElementById(tab);
	// let tabContentItem = document.getElementById(tabContent);
	// tabContentItem.classList.add('active');
	for (let i = 0; i<allTabs.length; i++) {
		if (allTabs[i].id !== tab) {
			allTabs[i].classList.remove('active');
		} else {
			allTabs[i].classList.add('active');
		}
		if (allTabsContent[i].id !== tabContent) {
			allTabsContent[i].classList.remove('active');
		} else {
			allTabsContent[i].classList.add('active');
		}
	}
}