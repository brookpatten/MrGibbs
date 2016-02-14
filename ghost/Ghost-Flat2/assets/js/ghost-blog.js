!function ($) {
  $(function(){

    $('[data-toggle="tooltip"]').tooltip();

	$("input[type='number']").stepper();

	//$(".selecter_1").selecter();
	//$(".selecter_2").selecter();

    $('.checkbox input').iCheck({
        checkboxClass: 'icheckbox_flat',
        increaseArea: '20%'
    });

    $('.radio input').iCheck({
        radioClass: 'iradio_flat',
        increaseArea: '20%'
    });
    //$('#accordion1').collapse();
    //$('#accordion2').collapse();
  })
}(window.jQuery)
