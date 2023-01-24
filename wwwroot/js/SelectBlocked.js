class Selector {
    constructor() {
        var input = document.getElementById("UserSearch");

        input.onchange = () => {
            
            var value = input.value;
            this.text = input.options[input.selectedIndex].text;

            var tbody = document.getElementById("BodyTable");
            var trs = tbody.querySelectorAll("tr");
            trs.forEach((tr) => {
                if (value === 'blocked')
                {
                    if (tr.cells[2].textContent === "False") {
                        tr.style.visibility = 'collapse';                   
                    }
                }
                else
                {
                    tr.style.visibility = 'visible';
                }
            })
        };
    }
}
var selector = new Selector();