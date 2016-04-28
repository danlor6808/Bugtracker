$(document).ready(function() {

    $('#datetimepicker').datetimepicker({
    });
    $('select').selectpicker();
    $("#phone").mask("(999) 999-9999");

    $('#collapse').on('click', function () {
        var width = $('.sidebar').width();
        $('.sidebar').toggle(function() {
            animate({'left': width + 'px'})
        });
        //$('.sidebar').toggleClass('col-sm-3 squeeze');
        //$('.sidebar').toggleClass('col-lg-2');
        $('.main').toggleClass('col-lg-10 col-lg-12');
        $('.main').toggleClass('col-lg-offset-2');
        $('.main').toggleClass('col-sm-9');
        $('.main').toggleClass('col-sm-offset-3');  
    });
});