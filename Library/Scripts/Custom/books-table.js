var currentPage;
var itemsPerPage;

var booksSort;
var sortBy;

var booksFilters;
var availableFilter;
var userBookFilter;

var tableContainer;

$(function () {
    // Initialization
    currentPage = sessionStorage.getItem('currentPage') == null ? 1 : sessionStorage.getItem('currentPage');
    itemsPerPage = 5;

    booksSort = $('#books_sort');
    sortBy = booksSort.val();

    booksFilters = $('[name="book-filters"]');
    checkFilters(booksFilters.filter(':checked').val());

    tableContainer = $('#table_container');
    //
    // Subscribes
    booksSort.on('change', function () {
        sortBy = $(this).val();
        loadBooks();
    });

    booksFilters.on('change', function () {
        checkFilters($(this).val());
        sessionStorage.setItem('currentPage', '1');
        currentPage = 1;
        loadBooks();
    });
    //
    loadBooks();
});

function loadBooks() {
    tableContainer.html(loader);
    $.ajax({
        method: 'GET',
        url: '/Book/TableBooks',
        data: {
            currentPage: currentPage,
            itemsPerPage: itemsPerPage,
            sortBy: sortBy,
            availableFilter: availableFilter,
            userBookFilter: userBookFilter
        },
        success: function (data) {
            tableContainer.html(data);
            pagination();
        }
    });
}

function pagination() {
    $('#books_paginator').on('click', 'a', function () {
        switch ($(this).attr('aria-label')) {
            case 'Previous':
                --currentPage;
                loadBooks();
                break;
            case 'Next':
                ++currentPage;
                loadBooks();
                break;
            case 'Current':
            default:
                break;
        }
        sessionStorage.setItem('currentPage', currentPage);
        return false;
    });
}

function checkFilters(value) {
    switch (value) {
        case 'available':
            availableFilter = true;
            userBookFilter = false;
            break;
        case 'user':
            availableFilter = false;
            userBookFilter = true;
            break;
        default:
            availableFilter = false;
            userBookFilter = false;
    }
}