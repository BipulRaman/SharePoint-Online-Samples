// Function to get User Derails based on ID. 
function getUserDetails(Id)
{      
	var returnValue;
	$.ajax({
		type: "GET",
		async: false,
		headers: {
			"accept": "application/json;odata=nometadata",
			"content-type": "application/json;odata=nometadata",
		},
		url: _spPageContextInfo.webAbsoluteUrl + '/_api/Web/SiteUserInfoList/Items('+ Id +')',
		success: function (data) {					
			 returnValue = data.Title;
		},
		failure: function (error) { console.log('Error in AJAX call to SP REST API: ' + error.message);}
	});
 return returnValue;
}

// Calling function to return value in ALERT
alert(getUserDetails(9));

// _spPageContextInfo.userId can be used to get current loggedIn UserID.
