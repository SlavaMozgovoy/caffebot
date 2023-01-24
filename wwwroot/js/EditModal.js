class EditDishModal {
    constructor(rootSelector, contentSelector) {
        this.root = document.querySelector(rootSelector);
        this.content = this.root.querySelector('.ModalWindow_content');

        this.contentParams = {
            title: this.content.querySelector('.Title'),
            form: this.content.querySelector('#EditDishForm'),
            name: this.content.querySelector('#EditDish_name'),
            description: this.content.querySelector('#EditDish_description'),
            price: this.content.querySelector('#EditDish_price'),
            categoryId: this.content.querySelector('#EditDish_categoryId'),
            delete: this.content.querySelector('#DeleteDishForm'),
            input: this.content.querySelector('#FileChanger')
        };

        this.content.onclick = (e) => e.stopPropagation();
        this.root.onclick = () => this.hideModal();
    }

    showModal(id, name, description, price, categoryId) {
        this.root.classList.add('active');
        let th = this;
        this.contentParams.form.action = `/Dish/EditDish/${id}`;
        this.contentParams.delete.action = `/Dish/DeleteDish/${id}`;
        this.contentParams.title.textContent = name;
        this.contentParams.name.value = name;
        this.contentParams.description.value = description;
        this.contentParams.price.value = parseFloat(price);
        this.contentParams.categoryId.value = categoryId;
        if (id === 0) {
            this.contentParams.delete.remove();
        }

        document.querySelector('body').classList.add('overflow-hiden');
    }
    hideModal() {
        this.root.classList.remove('active');
        document.querySelector('body').classList.remove('overflow-hiden');
    }
}

let modalDish = new EditDishModal('#EditDishModal');

class AddCategoryModal {
    constructor(rootSelector) {
        this.root = document.querySelector(rootSelector);
        this.content = this.root.querySelector('.ModalWindow_content');

        this.contentParams = {
            form: this.content.querySelector('#NewCategoryForm'),
        };
        this.contentParams.form.action = "/Dish/AddCategory/";
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


let addCategoryModal = new AddCategoryModal('#AddCategoryModal');

