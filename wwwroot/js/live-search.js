class LiveSearcher {
    constructor(input, elements, searchValue) {
        this.input = document.querySelector(input);

        this.elements = document.querySelectorAll(elements);
        this.searchResult = this.elements;
        this.searchValue = searchValue;


        this.debouncedSearch = this.debounce(this.liveSearch, 500);

        this.input.oninput = (e) => {
            this.debouncedSearch(e.target.value);
        }
    }

    get inputValue() {
        return this.input.value;
    }
    set Elements(elems) {
        this.elements = elems;
    }

    debounce(func, ms) {
        let timeout;
        return function () {
            const fnCall = () => {
                func.apply(this, arguments)
            }

            clearTimeout(timeout);
            timeout = setTimeout(fnCall, ms);
        };
    }

    getSearchValue(element) {
        return element.querySelector(this.searchValue).textContent;
    };

    liveSearch = (value) => {
        let inputValue = value;

        let searchResult = [];

        console.log(this.elements);

        if (inputValue != '') {
            this.elements.forEach(element => {
                if (this.getSearchValue(element).search(inputValue) == -1) {
                    element.classList.add('hide');
                    
                } else {
                    element.classList.remove('hide');
                    searchResult.push(element);
                }
            })
        }
        else {
            this.elements.forEach(function (element) {
                element.classList.remove('hide');
            });
        }
        this.searchResult = searchResult;
    }

    getSearchResult() {
        return Array.from(this.elements).filter(element => {
            console.log(!element.classList.contains('hide'));
            if (element.classList.contains('hide')) {
                return false;
            }
            else {
                return true;
            }
        });
    }
}

class LiveSelector {
    constructor(select, elements, searchValue) {
        this.select = document.querySelector(select);
        this.elements = document.querySelectorAll(elements);

        this.searchValue = searchValue;
        this.selectValue = this.select.value;

        this.select.onchange = () => {
            this.selectValue = this.select.value;

            this.elements.forEach(element => {
                let targetValue = this._getSearchValue(element).getAttribute('data-value');

                if (this._isAvailable(this.selectValue, targetValue)) {
                    element.classList.remove('hide');
                }
                else {
                    element.classList.add('hide');
                }
            });
        }

    }


    _getSearchValue(element) {
        return element.querySelector(this.searchValue);
    };

    _isAvailable(select, target) {
        if (select == 'allUsers') return true;
        if (select == 'blocked' && select == target) return true;
        return false;
    }
    getSortedElements() {
        return [...this.elements].filter(element => !element.classList.contains('hide'));
    }
}

class SelectSearcher {
    constructor(liveSelect, liveSearch) {
        this.liveSelect = liveSelect;
        this.liveSearch = liveSearch;

        this.liveSearch.Elements = this.liveSelect.getSortedElements();

        this.liveSelect.select.addEventListener('change', () => {
            this.liveSearch.Elements = this.liveSelect.getSortedElements();
            this.liveSearch.liveSearch(this.liveSearch.inputValue);
        })
    }

}

const selectedAndSearched = new SelectSearcher(
    new LiveSelector('#UserSearch', '#UsersTable tbody tr', '.UserName'),
    new LiveSearcher('#AllUsersSeach', '#UsersTable tbody tr', '.UserName'),
);
