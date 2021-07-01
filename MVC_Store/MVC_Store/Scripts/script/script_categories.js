const open_btn = document.querySelector('.open-btn')
const close_btn = document.querySelector('.close-btn')
const nav = document.querySelectorAll('.nav-white')
const logo_btn = document.querySelector('.navbar-brand')
const full_screen = document.querySelector('.full-screen')
const resons_why = document.querySelector('.reasons-why')




open_btn.addEventListener('click', () => {
    nav.forEach(nav_el => nav_el.classList.add('visible'))
})

close_btn.addEventListener('click', () => {
    nav.forEach(nav_el => nav_el.classList.remove('visible'))
})

open_btn.addEventListener('click', () => {
    navbar.classList.add("no-transparent")
})
close_btn.addEventListener('click', () => {
    navbar.classList.remove("no-transparent")
})

logo_btn.addEventListener('click', () => {
    navbar.classList.add("sticky");
})

window.onscroll = function () { myFunction() };

var navbar = document.getElementById("navbar");
var sticky = navbar.offsetTop;
function myFunction() {
    if (window.pageYOffset > sticky ) {
        navbar.classList.add("sticky")
    } else{
        navbar.classList.remove("sticky");
    }  
}
function isPartiallyVisible(el) {
    var elementBoundary = el.getBoundingClientRect();

    var top = elementBoundary.top;
    var bottom = elementBoundary.bottom;
    var height = elementBoundary.height;

    return ((top + height >= 0) && (height + window.innerHeight >= bottom));
}

var isScrolling = false;

window.addEventListener("scroll", throttleScroll, false);

function throttleScroll(e) {
    if (isScrolling == false) {
        window.requestAnimationFrame(function () {
            scrolling(e);
            isScrolling = false;
        });
    }
    isScrolling = true;
}

document.addEventListener("DOMContentLoaded", scrolling, false);

function scrolling(e) {

    if (isPartiallyVisible(full_screen)) {
        navbar.classList.add("transparent");
    } else {
        navbar.classList.remove("transparent");
    }
}