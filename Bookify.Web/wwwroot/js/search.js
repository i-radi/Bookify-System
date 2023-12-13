$(document).ready(function () {
    var books = new Bloodhound({
        datumTokenizer: Bloodhound.tokenizers.obj.whitespace('value'),
        queryTokenizer: Bloodhound.tokenizers.whitespace,
        remote: {
            url: '/Search/Find?query=%QUERY',
            wildcard: '%QUERY'
        }
    });

    $('#Search').typeahead({
        minLength: 4,
        highlight: true
    }, {
        name: 'book',
        limit: 100,
        display: 'title',
        source: books,
        templates: {
            //header: '<h3 class="p-2">Books</h3>',
            empty: [
                '<div class="m-3 fw-bold">',
                localization.noBooksFound,
                '</div>'
            ].join('\n'),
            suggestion: Handlebars.compile('<div class="py-2"><span>{{title}}</span><br/><span class="f-xs text-gray-400">by {{author}}</span></div>')
        }
    }).on('typeahead:select', function (e, book) {
        window.location.replace(`/Search/Details?bKey=${book.key}`);
    });
});