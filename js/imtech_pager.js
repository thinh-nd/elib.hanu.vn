var Imtech = {};
Imtech.Pager = function() {
    this.paragraphsPerPage = 3;
    this.currentPage = 1;
    this.pageLinkEachSide = 3;
    this.pagingControlsContainer = ".pagination";
    this.pagingContainerPath = ".pagination1";
    
    this.numPages = function() {
        var numPages = 0;
        if (this.paragraphs != null && this.paragraphsPerPage != null) {
            numPages = Math.ceil(this.paragraphs.length / this.paragraphsPerPage);
        }        
        return numPages;
    };
    
    this.showPage = function(page) {
        this.currentPage = page;
        var html = "";
        for (var i = (page-1)*this.paragraphsPerPage; i < ((page-1)*this.paragraphsPerPage) + this.paragraphsPerPage; i++) {
            if (i < this.paragraphs.length) {
                var elem = this.paragraphs.get(i);
                html += "<" + elem.tagName + ">" + elem.innerHTML + "</" + elem.tagName + ">";
            }
        }
        
        $(this.pagingContainerPath).html(html);

        renderControls(this.pagingControlsContainer, this.currentPage, this.numPages(), this.pageLinkEachSide);
    }

    var renderControls = function (container, currentPage, numPages, pagesCutoff) {
        var pagingControls = "<b>Trang</b>: <ul>";
        var prevPosition = currentPage - pagesCutoff;
        var nextPosition = currentPage + pagesCutoff;

        for (var i = 1; i <= numPages; i++) {
            if (i != currentPage) {
                if (i >= prevPosition && i <= nextPosition) {
                    var linkToPage = i == prevPosition ? currentPage - 1 : i == nextPosition ? currentPage + 1 : i;
                    var linkText = i == prevPosition ? "<< prev" : i == nextPosition ? "next >>" : i;

                    pagingControls += "<li><a href='#' onclick='pager.showPage(" + linkToPage + "); return false;'>" + linkText + "</a></li>";
                }
            } else {
                pagingControls += "<li>" + i + "</li>";
            }
        }

        pagingControls += "</ul>";

        $(container).html(pagingControls);
    }
}