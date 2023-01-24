let rangeInput = document.querySelector('#ReportRange');

let rangeDt = new AirDatepicker(rangeInput, {
    buttons: ['today', 'clear'],
    autoClose: true,
    isMobile: window.innerWidth <= 600 ? true : false,
    toggleSelected: false,
    range: true,
    multipleDatesSeparator: '-',
})


let btn = document.querySelector('#MakeReport');

btn.onclick = () => {
    if(!rangeInput.value) return;
    window.open(getUrl(...getRange(rangeInput.value)), '_blank');
}



function getRange(inputValue) {
    let dates = inputValue.split('-');
    return dates.length > 1 ? dates : [dates[0], dates[0]];
}

function getUrl(from, to) {
    return `https://nashecaffe-bot.site/api/getPeriod?from=${from}&to=${to}`;
}