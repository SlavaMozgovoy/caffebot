class AsyncTableLoader {
    constructor(rootSelector) {
        document.onload = async () => {
            this.getClosedOrdersAsync();
        }
        this.root = document.querySelector(rootSelector);
    }
    async getClosedOrdersAsync(date) {
        this.root.innerHTML = '';
        const response = await (await fetch(`/api/getOrdersByDate?date=${date}`)).text();
        this.root.innerHTML = response;
    }
    getChildeElements() {
        return this.root.children;
    }
}

let inputDate = document.querySelector('#closedChecks_Date input');

let inputCashe = null;

let dt = new AirDatepicker(inputDate, {
    buttons: ['today', 'clear'],
    autoClose: true,
    isMobile: window.innerWidth <= 600 ? true : false,
    onSelect: onSelect,
    toggleSelected: false,
});


var nameSearch = new LiveSearcher('#LiveSearcherInput', '#tableUsers tr', '.UserName');
dt.selectDate(new Date());

const tableLoader = new AsyncTableLoader('#tableUsers');


async function onSelect(data) {
    let value = data.formattedDate;

    await tableLoader.getClosedOrdersAsync(value);
    nameSearch.Elements = Array.from(tableLoader.getChildeElements());
    nameSearch.liveSearch(nameSearch.inputValue);
}