let placeholderList = document.querySelectorAll('.AwesomeTextArea');
placeholderList.forEach(element => {
    let input = element.querySelector('textarea');

    input.onfocus = () => {
        element.classList.add('active');
    }
    input.onblur = () => {
        if(input.value) return;
        element.classList.remove('active');
    }
});

let fileInputs = document.querySelectorAll(".AwesomeFileInput");
fileInputs.forEach(element => {
    let input = element.querySelector('input[type="file"]');
    let state = element.querySelector('.AwesomeFileInput_state');
    input.addEventListener('change', function(e) {
        if(!this.value) {
            state.textContent = "Файл не выбран";
            return;
        }

        state.textContent = this.value.replace("C:\\fakepath\\", "");
    });
});