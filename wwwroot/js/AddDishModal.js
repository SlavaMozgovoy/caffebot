class AddDishModal {
    constructor(rootSelector) {
        this.root = document.querySelector(rootSelector);
        this.content = this.root.querySelector('.ModalWindow_content');
        // тут иницализация если нужно параметров по примеру

        // this.contentParams = {
        //     form: this.content.querySelector('#NewCategoryForm'),
        // };
        this.content.onclick = (e) => e.stopPropagation();
        this.root.onclick = () => this.hideModal();

    }

    showModal(/* тут параметры если надо в модалку передать */) {
        this.root.classList.add('active');

        // тут добавление этих параметров если нужно

        document.querySelector('body').classList.add('overflow-hiden');
    }
    hideModal() {
        this.root.classList.remove('active');
        document.querySelector('body').classList.remove('overflow-hiden');
    }
}