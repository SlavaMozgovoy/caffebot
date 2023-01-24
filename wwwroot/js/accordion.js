class Accordion {
    constructor(root, label, content) {
        this.root = document.querySelector(root);
        console.log(root + label + content);
        this.labels = this.root.querySelectorAll(label);
        this.contets = this.root.querySelectorAll(content);

        this._addEvents();
    }


    _addEvents = () => {
        this.labels.forEach(label => {
            label.onclick = () => {
                let content = label.nextElementSibling;

                if (content.style.maxHeight) {
                    content.style.maxHeight = null;
                    return;
                }

                this.contets.forEach(element => {
                    element.style.maxHeight = null;
                })

                content.style.maxHeight = content.scrollHeight + 'px';
            }
        });
    }
}


   