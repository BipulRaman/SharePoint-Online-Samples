function getUserDetails(Id)
{
	var strListTitle = 'User Information List';
	var viewXml = "<View><Query><Where><Eq><FieldRef Name='ID' /><Value Type='Counter'>"+Id+"</Value></Eq></Where></Query></View>";        
	var returnValue;
	$.ajax({
		type: "POST",
		async: false,
		headers: {
			"accept": "application/json;odata=nometadata",
			"content-type": "application/json;odata=nometadata",
			"X-RequestDigest": $("#__REQUESTDIGEST").val()
		},
		url: _spPageContextInfo.webAbsoluteUrl + '/_api/web/Lists/GetByTitle(\''+strListTitle +'\')/GetItems(query=@v1)?@v1={"ViewXml":"'+viewXml+'"}',
		success: function (data) {					
			 returnValue = data.value[0]["Title"];
		},
		failure: function (error) { console.log('Error in AJAX call to SP REST API: ' + error.message);}
	});
  return returnValue;
}


// _spPageContextInfo.userId can be used to get current loggedIn UserID. 
