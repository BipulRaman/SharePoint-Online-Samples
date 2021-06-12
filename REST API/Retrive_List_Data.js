var strListTitle = "MY LIST NAME";
var strListName = "MY_x0020_LIST_x0020_NAME";
// var viewXml = "<View Scope='RecursiveAll'><Query><OrderBy><FieldRef Name='Title' Ascending='False' /></OrderBy></Query></View>";   
var viewXml = "YOUR_CAML_QUERY";

$.ajax({
	type: "POST",
	async: true,
	headers: {
		"accept": "application/json;odata=verbose",
		"content-type": "application/json;odata=verbose",
		"X-RequestDigest": _spPageContextInfo.formDigestValue
	},
	url: _spPageContextInfo.webAbsoluteUrl + '/_api/web/Lists/GetByTitle(\'' + strListTitle + '\')/GetItems(query=@v1)?@v1={"ViewXml":"' + viewXml + '"}',
	success: function (response) {
		data = response.d.results;
		data.forEach(function (item) {
			console.log(item.Title);
		});
	},
	failure: function (error) { 
		console.log('Error in AJAX call to SP REST API: ' + error.message); 
	}
});
