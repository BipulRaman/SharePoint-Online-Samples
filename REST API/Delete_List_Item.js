var strListTitle = "YOUR_LIST_NAME";
var ID = "1"; // Item Id to delete
$.ajax({
    type: "POST",
    async: true,
    headers: {
        "accept": "application/json;odata=nometadata",
        "content-type": "application/json;odata=nometadata",
        "X-RequestDigest": $("#__REQUESTDIGEST").val(),
         "IF-MATCH": "*",
        "X-HTTP-Method":"DELETE"
    },
    url: _spPageContextInfo.webAbsoluteUrl + '/_api/web/Lists/GetByTitle(\''+strListTitle +'\')/items('+ ID +')',
    success: function (data) {	
    },
    failure: function (error) { console.log('Error in AJAX call to SP REST API: ' + error.message);}
});
