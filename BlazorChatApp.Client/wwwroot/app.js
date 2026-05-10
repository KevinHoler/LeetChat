window.getFromStorage = function (key) {
    return localStorage.getItem(key);
};

window.setInStorage = function (key, value) {
    localStorage.setItem(key, value);
};