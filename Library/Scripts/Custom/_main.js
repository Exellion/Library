var renderBody = $('#render_body');
var relativePath = window.location.href.toString().replace((window.location.protocol + '//' + window.location.host + '/'), '');

$(function () {
    if (relativePath != '' && relativePath.indexOf('ShowBookInfo') == -1)
        sessionStorage.removeItem('currentPage');
})