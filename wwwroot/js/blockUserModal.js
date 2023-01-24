class BlockUserModal {
    constructor() {
        this.root = document.querySelector(".ModalWindow");
        this.content = this.root.querySelector('.ModalWindow_content');

        this.content.onclick = (e) => e.stopPropagation();
        this.root.onclick = () => this.hideModal();
    }

    showModal() {
        this.root.classList.add('active');

        document.querySelector('body').classList.add('overflow-hiden');
    }
    hideModal() {
        this.root.classList.remove('active');
        document.querySelector('body').classList.remove('overflow-hiden');
    }
}

let blockUserModal = new BlockUserModal();