/*-------------------------------------------------
Quick Pager jquery plugin
		
Copyright (C) 2011 by Dan Drayne

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
		
v1.1/		18/09/09 * bug fix by John V - http://blog.geekyjohn.com/
-------------------------------------------------*/

(function ($) {

    $.fn.quickPager = function (options) {

        var defaults = {
            pageSize: 10,
            currentPage: 1,
            holder: null,
            pagerLocation: "after"
        };

        var options = $.extend(defaults, options);


        return this.each(function () {


            var selector = $(this);
            var pageCounter = 1;

            selector.wrap("<div class='simplePagerContainer'></div>");

            selector.children().each(function (i) {

                if (i < pageCounter * options.pageSize && i >= (pageCounter - 1) * options.pageSize) {
                    $(this).addClass("simplePagerPage" + pageCounter);
                }
                else {
                    $(this).addClass("simplePagerPage" + (pageCounter + 1));
                    pageCounter++;
                }

            });

            // show/hide the appropriate regions 
            selector.children().hide();
            selector.children(".simplePagerPage" + options.currentPage).show();

            if (pageCounter <= 1) {
                return;
            }

            //Build pager navigation
            var pageNav = " <ul class='simplePagerNav'>Trang: ";
            for (i = 1; i <= pageCounter; i++) {
                if (i == options.currentPage) {
                    pageNav += "<li class='currentPage simplePageNav" + i + "'><a rel='" + i + "' href='#'>" + i + "</a></li>";
                }
                else {
                    pageNav += "<li class='simplePageNav" + i + "'><a rel='" + i + "' href='#'>" + i + "</a></li>";
                }
            }
            pageNav += "</ul>";

            if (!options.holder) {
                switch (options.pagerLocation) {
                    case "before":
                        selector.before(pageNav);
                        break;
                    case "both":
                        selector.before(pageNav);
                        selector.after(pageNav);
                        break;
                    default:
                        selector.after(pageNav);
                }
            }
            else {
                $(options.holder).append(pageNav);
            }

            //max visible page at startup(tức hiện tất cả người dùng),nếu số trang > 5 thì hiện 5 trang đầu,<1 thì hiện 1 trang, 1<pC<5
            //thì hiện pageCounter-1 trang

            if (pageCounter >= 5) {
                $(".all .simplePagerNav li").hide();
                $(".all .simplePagerNav li:lt(5)").show();
            }          
            //dropdownlist user option change
            $(".all").show(); $(".user").hide(); $(".thuthu").hide(); $(".none").hide();
            $("#optionUser").change(function () {
                $(".all").hide(); $(".user").hide(); $(".thuthu").hide(); $(".none").hide();
                $(".all,.user,.thuthu,.none").removeClass("active");
                if ($(this).val() == "all") {
                    $(".all").show();
                    $(".simplePagerNav li").detach();

                    $(".simplePagerNav").append("<li class='currentPage simplePageNav1'><a rel='1' href='#'>1</a></li>");
                    var relNum = 1;
                    if (relNum + 1 <= pageCounter) {
                        $(".simplePagerNav").append("<li class='simplePageNav" + (relNum + 1) + "'><a rel='" + (relNum + 1) + "' href='#'>" + (relNum + 1) + "</a></li>");
                    }
                    if (relNum + 2 <= pageCounter) {
                        $(".simplePagerNav").append("<li class='simplePageNav" + (relNum + 2) + "'><a rel='" + (relNum + 2) + "' href='#'>" + (relNum + 2) + "</a></li>");
                    }


                } else if ($(this).val() == "user") {
                    $(".user").show();
                    $(".simplePagerNav li").detach();

                    $(".simplePagerNav").append("<li class='currentPage simplePageNav1'><a rel='1' href='#'>1</a></li>");
                    var relNum = 1;
                    if (relNum + 1 <= pageCounter) {
                        $(".simplePagerNav").append("<li class='simplePageNav" + (relNum + 1) + "'><a rel='" + (relNum + 1) + "' href='#'>" + (relNum + 1) + "</a></li>");
                    }
                    if (relNum + 2 <= pageCounter) {
                        $(".simplePagerNav").append("<li class='simplePageNav" + (relNum + 2) + "'><a rel='" + (relNum + 2) + "' href='#'>" + (relNum + 2) + "</a></li>");
                    }

                } else if ($(this).val() == "thuthu") {
                    $(".thuthu").show();
                    $(".simplePagerNav li").detach();

                    $(".simplePagerNav").append("<li class='currentPage simplePageNav1'><a rel='1' href='#'>1</a></li>");
                    var relNum = 1;
                    if (relNum + 1 <= pageCounter) {
                        $(".simplePagerNav").append("<li class='simplePageNav" + (relNum + 1) + "'><a rel='" + (relNum + 1) + "' href='#'>" + (relNum + 1) + "</a></li>");
                    }
                    if (relNum + 2 <= pageCounter) {
                        $(".simplePagerNav").append("<li class='simplePageNav" + (relNum + 2) + "'><a rel='" + (relNum + 2) + "' href='#'>" + (relNum + 2) + "</a></li>");
                    }

                } else {
                    $(".none").show();
                    $(".simplePagerNav li").detach();

                    $(".simplePagerNav").append("<li class='currentPage simplePageNav1'><a rel='1' href='#'>1</a></li>");
                    var relNum = 1;
                    if (relNum + 1 <= pageCounter) {
                        $(".simplePagerNav").append("<li class='simplePageNav" + (relNum + 1) + "'><a rel='" + (relNum + 1) + "' href='#'>" + (relNum + 1) + "</a></li>");
                    }
                    if (relNum + 2 <= pageCounter) {
                        $(".simplePagerNav").append("<li class='simplePageNav" + (relNum + 2) + "'><a rel='" + (relNum + 2) + "' href='#'>" + (relNum + 2) + "</a></li>");
                    }

                }

            });
            //pager navigation behaviour
            selector.parent().find(".simplePagerNav").delegate("li a[rel]", "click", function () {

                //grab the REL attribute 
                var clickedLink = $(this).attr("rel");
                var relNum = parseInt(clickedLink);
                options.currentPage = clickedLink;

                if (options.holder) {
                    $(this).parent("li").parent("ul").parent(options.holder).find("li.currentPage").removeClass("currentPage");
                    $(this).parent("li").parent("ul").parent(options.holder).find("a[rel='" + clickedLink + "']").parent("li").addClass("currentPage");
                }
                else {
                    //remove current current (!) page
                    $(this).parent("li").parent("ul").parent(".simplePagerContainer").find("li.currentPage").removeClass("currentPage");
                    //Add current page highlighting
                    $(this).parent("li").parent("ul").parent(".simplePagerContainer").find("a[rel='" + clickedLink + "']").parent("li").addClass("currentPage");
                }

                //visible page
                //get number of pages where class is active                                    

                $(".simplePagerNav li").detach();

                if (relNum - 2 > 0) {
                    $(".simplePagerNav").append("<li class='simplePageNav" + (relNum - 2) + "'><a rel='" + (relNum - 2) + "' href='#'>" + (relNum - 2) + "</a></li>");
                }
                if (relNum - 1 > 0) {
                    $(".simplePagerNav").append("<li class='simplePageNav" + (relNum - 1) + "'><a rel='" + (relNum - 1) + "' href='#'>" + (relNum - 1) + "</a></li>");
                }
                $(".simplePagerNav").append("<li class='currentPage simplePageNav" + (relNum) + "'><a rel='" + (relNum) + "' href='#'>" + (relNum) + "</a></li>");
                if (relNum + 1 <= pageCounter) {
                    $(".simplePagerNav").append("<li class='simplePageNav" + (relNum + 1) + "'><a rel='" + (relNum + 1) + "' href='#'>" + (relNum + 1) + "</a></li>");
                }
                if (relNum + 2 <= pageCounter) {
                    $(".simplePagerNav").append("<li class='simplePageNav" + (relNum + 2) + "'><a rel='" + (relNum + 2) + "' href='#'>" + (relNum + 2) + "</a></li>");
                }


                //hide and show relevant links
                selector.children().hide();
                selector.find(".simplePagerPage" + clickedLink).show();

                return false;
            });

            //prev/next            

        });



    }

})(jQuery);

