$(document).ready(function() {

    $('#datetimepicker').datetimepicker({
        dateFormat: "dd/mm/yy",
        showStatus: true,
        showWeeks: true,
        currentText: 'Now',
        autoSize: true,
        gotoCurrent: true,
        showAnim: 'blind',
        highlightWeek: true
    });
    $('select').selectpicker();
    $("#phone").mask("(999) 999-9999");
});