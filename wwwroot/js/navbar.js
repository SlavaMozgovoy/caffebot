class Navbar {
    constructor(root, toggleBtn, navbarList) {
        this.root = document.querySelector(root);
        console.log(this.root);

        this.toggleBtn = this.root.querySelector(toggleBtn);
        this.navbar = this.root.querySelector(navbarList);
        this.navbarItems = this.navbar.querySelectorAll('ul li');

        this.addEvents();
    }

    get isMobile() {
        return this.getNavbarWidth() >= this.getAvailableWidth();
    }

    get isActive() {
        return this.navbar.classList.contains('active');
    }

    addEvents() {
        document.onclick = (e) => {
            if (e.target !== this.root && e.target !== this.toggleBtn && e.target !== this.navbar) {
                this.toggleBtn.classList.remove('active');
                this.navbar.classList.remove('active');
            }
        }
        this.toggleBtn.onclick = () => {
            this.navbar.classList.toggle('active');
            this.toggleBtn.classList.toggle('active');

            if (this.isMobile) {
                this.navbar.classList.add('mobile');
            }
            else {
                this.navbar.classList.remove('mobile');
            }
        }
    }

    getAvailableWidth() {
        return this.root.clientWidth - this.root.querySelector('.logo').clientWidth - this.toggleBtn.clientWidth - 60;
    }

    getNavbarWidth() {
        let total = 0;
        this.navbarItems.forEach(element => {
            total += element.clientWidth;
        });
        return total;
    }
}

let nav = new Navbar('#header', '#toggle', '#navbar');