$(document).ready(function () {
    $("#bookshelf ul li,#search-result li .bookDetailInfo div").hover(
		function () {
		    $(this).children(".book_info").fadeIn("medium");
		},

		function () {
		    $(this).children(".book_info").fadeOut("medium");
		}
	);
			

    $(".categogies").click(
		function () {
		    $(".cat_list").toggle(0);
		}
	);

    $(".formats").click(
		function () {
		    $(".format_list").toggle(0);
		}
	);

    $(".mostviewed").click(
		function () {
		    $(".mv_list").toggle(0);
		}
	);

    $(".search").fadeIn("slow");
});

function view_confirmation(fee) {
	if (fee != 0) {
		alert('Tài liệu có giá ' + fee + ' VNÐ sẽ được trừ vào tài khoản của bạn.');
	}
}