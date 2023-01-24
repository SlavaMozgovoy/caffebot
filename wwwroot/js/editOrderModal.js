class EditOrderModal {
    constructor() {
        this.root = document.querySelector("#EditOrderModal");
        this.content = this.root.querySelector('.ModalWindow_content');

        this.contentParams = {
            title: this.content.querySelector(".Title"),
            form: this.content.querySelector("#EditPositions"),
            count: this.content.querySelector(".InputWithIcon_input")
        };

        this.content.onclick = (e) => e.stopPropagation();
        this.root.onclick = () => this.hideModal();
    }

    showModal(id, title, count) {
        this.root.classList.add('active');

        this.contentParams.form.action = `/Order/EditDishCount/${id}`;
        this.contentParams.title.innerHTML = title;
        this.contentParams.count.value = count;

        document.querySelector('body').classList.add('overflow-hiden');
    }
    hideModal() {
        this.root.classList.remove('active');
        document.querySelector('body').classList.remove('overflow-hiden');
    }
}

let editOrderModal = new EditOrderModal();