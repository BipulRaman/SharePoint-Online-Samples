$.ajax({
	type: "POST",
	async: true,
	headers: {
		"accept": "application/json;odata=nometadata",
		"content-type": "application/json;odata=nometadata",
		"X-RequestDigest": $("#__REQUESTDIGEST").val()
	},
	url: _spPageContextInfo.webAbsoluteUrl + '/_api/SP.UserProfiles.PeopleManager/GetMyProperties',
	success: function (data) {
		console.log(data.Title);
	},
	failure: function (error) { console.log('Error in AJAX call to SP REST API: ' + error.message);}
});